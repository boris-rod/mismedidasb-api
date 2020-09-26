using Amazon;
using Amazon.CloudWatchLogs;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MismeAPI.Data;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.AwsCloudWatch;
using System;
using System.IO;
using System.Text;

namespace MismeAPI
{
    public static class ServicesExtensions
    {
        public static void ConfigureDbContext(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContextPool<MismeContext>(
                options => options.UseMySql(config.GetConnectionString("ApiConnection"),
                mySqlOptions =>
                {
                    // replace with your Server Version and Type
                    mySqlOptions.ServerVersion(new Version(5, 7), ServerType.MySql)
                    .CharSetBehavior(CharSetBehavior.AppendToAllColumns)
                    .DisableBackslashEscaping()
                    .CharSet(CharSet.Utf8Mb4);
                }
            ));
        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                // define swagger docs and other options
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Misme API",
                    Version = "v1",
                    Description = "Misme API"
                });
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter JWT Bearer authorization token",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // must be lowercase!!!
                    BearerFormat = "Bearer {token}",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        // defines scope - without a protocol use an empty array for global scope
                        { securityScheme, Array.Empty<string>() }
                    }
                );
                var basePath = AppContext.BaseDirectory;
                var fileName = Path.Combine(basePath, "MismeAPI.xml");
                var fileName2 = Path.Combine(basePath, "MismeAPI.Common.xml");
                options.IncludeXmlComments(fileName);
                options.IncludeXmlComments(fileName2);
            });
        }

        public static void ConfigureCompression(this IServiceCollection services)
        {
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression();
        }

        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                    builder.SetIsOriginAllowed(_ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
        }

        public static void ConfigureTokenAuth(this IServiceCollection services, IConfiguration config)
        {
            var key = Encoding.UTF8.GetBytes(config.GetSection("BearerTokens")["Key"]);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = config.GetSection("BearerTokens")["Audience"],
                    ValidateAudience = true,
                    ValidIssuer = config.GetSection("BearerTokens")["Issuer"],
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true
                };
            });
        }

        public static void ConfigureDetection(this IServiceCollection services)
        {
            services.AddDetection();
        }

        public static void ConfigureSignalR(this IServiceCollection services)
        {
            services.AddSignalR();
        }

        public static void ConfigureHangfire(this IServiceCollection services, IConfiguration config)
        {
            services.AddHangfire(configuration => configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseStorage(new MySqlStorage(config.GetConnectionString("HangfireConnection"),
                       new MySqlStorageOptions
                       {
                           //TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                           QueuePollInterval = TimeSpan.FromSeconds(15),
                           JobExpirationCheckInterval = TimeSpan.FromHours(1),
                           CountersAggregateInterval = TimeSpan.FromMinutes(5),
                           PrepareSchemaIfNecessary = false,
                           DashboardJobListLimit = 50000,
                           TransactionTimeout = TimeSpan.FromMinutes(1),
                           TablesPrefix = "Hangfire"
                       })));

            // Add the processing server as IHostedService
            services.AddHangfireServer();
        }

        public static void ConfigureLogs(this IServiceCollection services, IConfiguration config)
        {
            //loggerConfiguration
            //    .MinimumLevel.Information()
            //    .Enrich.FromLogContext()
            //    .WriteTo.Console();

            if (!string.IsNullOrWhiteSpace(config.GetSection("AWS")["CloudWatchDev"]))
            {
                var options = new CloudWatchSinkOptions
                {
                    LogGroupName = config.GetSection("AWS")["CloudWatchDev"],
                    CreateLogGroup = true,
                    MinimumLogEventLevel = LogEventLevel.Information,
                    TextFormatter = new CompactJsonFormatter(),
                    BatchSizeLimit = 100,
                    QueueSizeLimit = 10000,
                    Period = TimeSpan.FromSeconds(10),
                    LogStreamNameProvider = new DefaultLogStreamProvider(),
                    RetryAttempts = 5
                };
                //var awsOptions = config.GetAWSOptions();

                var accessKey = config["AWS:AccessKey"];
                var secretKey = config["AWS:SecretKey"];
                var cloudWatchClient = new AmazonCloudWatchLogsClient(accessKey, secretKey, RegionEndpoint.EUWest1);

                //var cloudWatchClient = awsOptions.CreateServiceClient<IAmazonCloudWatchLogs>();

                Log.Logger = new LoggerConfiguration()
                             .MinimumLevel.Error()
                             .Enrich.FromLogContext()
                             .WriteTo.Console()
                             .WriteTo.AmazonCloudWatch(options, cloudWatchClient)
                             .CreateLogger();
            }
        }
    }
}
