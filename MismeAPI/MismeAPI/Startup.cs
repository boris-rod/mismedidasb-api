using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using MismeAPI.Data.Repository;
using MismeAPI.Data.UoW;
using MismeAPI.Filters;
using MismeAPI.Middlewares;
using MismeAPI.Service;
using MismeAPI.Service.Hubs;
using MismeAPI.Service.Impls;
using MismeAPI.Service.Utils;
using MismeAPI.Services;
using MismeAPI.Services.Impls;
using MismeAPI.Utils;
using Newtonsoft.Json;
using Stripe;
using System;

namespace MismeAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //Log.Logger = new LoggerConfiguration()
            //             .MinimumLevel.Information()
            //             //.WriteTo.File(AppDomain.CurrentDomain.BaseDirectory + "\\logs\\mismeapi-.log", rollingInterval: RollingInterval.Day)
            //             .WriteTo.File("./logs/mismeapi-.log", rollingInterval: RollingInterval.Day)
            //             .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureLogs(Configuration);

            services.ConfigureCors();
            services.ConfigureSignalR();
            services.ConfigureHangfire(Configuration);

            services.ConfigureDbContext(Configuration);
            services.ConfigureSwagger();
            services.ConfigureTokenAuth(Configuration);
            services.AddResponseCaching();
            services.ConfigureCompression();
            services.ConfigureHealthChecks(Configuration);
            services.ConfigureDetection();

            services.AddHttpContextAccessor();
            services.AddMvc(config =>
            {
                config.Filters.Add(typeof(ApiValidationFilterAttribute));
                config.EnableEndpointRouting = false;
            }).AddNewtonsoftJson(opt => opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddTransient<IAmazonS3Service, AmazonS3Service>();
            services.AddTransient<IFileService, Services.Impls.FileService>();
            services.AddTransient<IAccountService, Services.Impls.AccountService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IPollService, PollService>();
            services.AddTransient<IQuestionService, QuestionService>();
            services.AddTransient<IConceptService, ConceptService>();
            services.AddTransient<IAnswerService, AnswerService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ITagService, TagService>();
            services.AddTransient<IDishService, DishService>();
            services.AddTransient<IEatService, EatService>();
            services.AddTransient<IDeviceService, DeviceService>();
            services.AddTransient<IMismeBackgroundService, MismeBackgroundService>();
            services.AddTransient<ITipService, TipService>();
            services.AddTransient<IReminderService, ReminderService>();
            services.AddTransient<IResultService, ResultService>();
            services.AddTransient<ISettingService, SettingService>();
            services.AddTransient<IGeneralContentService, GeneralContentService>();
            services.AddTransient<IContactUsService, ContactUsService>();
            services.AddTransient<IRewardCategoryService, RewardCategoryService>();
            services.AddTransient<IRewardService, RewardService>();
            services.AddTransient<IUserStatisticsService, UserStatisticsService>();
            services.AddTransient<ICompoundDishService, CompoundDishService>();
            services.AddTransient<ICutPointService, CutPointService>();
            services.AddTransient<IRewardHelper, RewardHelper>();
            services.AddTransient<IUserReferralService, UserReferralService>();
            services.AddTransient<IScheduleService, ScheduleService>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<ISoloQuestionService, SoloQuestionService>();
            services.AddTransient<ISubscriptionService, MismeAPI.Service.Impls.SubscriptionService>();
            services.AddTransient<IAppService, AppService>();
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient<IPaypalService, PaypalService>();
            services.AddTransient<IProductService, Services.Impls.ProductService>();

            var provider = services.BuildServiceProvider();
            var amazonS3Service = provider.GetService<IAmazonS3Service>();
            var userStatisticsService = provider.GetService<IUserStatisticsService>();
            var contextAccessor = provider.GetService<IHttpContextAccessor>();

            var apiMappings = new MappingProfiles(amazonS3Service, userStatisticsService, contextAccessor);
            services.AddAutoMapper(x => x.AddProfile(apiMappings), typeof(Startup));

            var stripeApiKey = Configuration.GetSection("Stripe")["ApiKey"];
            StripeConfiguration.ApiKey = stripeApiKey;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services, IBackgroundJobClient backgroundJobs, IRecurringJobManager recurringJobs)
        {
            app.UseCors();
            app.UseHangfireDashboard();
            var mismeBackJobs = services.GetRequiredService<IMismeBackgroundService>();
            recurringJobs.AddOrUpdate<IMismeBackgroundService>("ExpiredTokens", (e) => e.CleanExpiredTokensAsync(), "0 3 * * *");
            recurringJobs.AddOrUpdate<IMismeBackgroundService>("DisabledAccount", (e) => e.RemoveDisabledAccountsAsync(), "0 3 * * *");
            //recurringJobs.AddOrUpdate<IMismeBackgroundService>("Notifications", (e) => e.SendFireBaseNotificationsRemindersAsync(), "0 18 * * *");
            recurringJobs.AddOrUpdate<IMismeBackgroundService>("HandleUserStreaksWest", (e) => e.HandleUserStreaksAsync(1), "0 10 * * *", TimeZoneInfo.Utc);
            recurringJobs.AddOrUpdate<IMismeBackgroundService>("HandleUserStreaksEast", (e) => e.HandleUserStreaksAsync(-1), "0 23 * * *", TimeZoneInfo.Utc);
            recurringJobs.AddOrUpdate<IMismeBackgroundService>("HandleSubscriptions", (e) => e.HandleSubscriptionsAsync(), "0 12 * * *", TimeZoneInfo.Utc);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Misme API V1");
            });

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware(typeof(ErrorWrappingMiddleware));
            app.UseResponseCompression();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<UserHub>("/userHub", map => { });

                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                });
                endpoints.MapControllers();
            });

            DatabaseSeed.SeedDatabaseAsync(services).Wait();
        }
    }
}
