using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using MismeAPI.Data;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using System;
using System.IO;

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
            services.AddCors();
        }
    }
}