using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.Repository;
using MismeAPI.Data.UoW;
using MismeAPI.Filters;
using MismeAPI.Middlewares;
using MismeAPI.Service;
using MismeAPI.Service.Hubs;
using MismeAPI.Service.Impls;
using MismeAPI.Services;
using MismeAPI.Services.Impls;
using MismeAPI.Utils;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MismeAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Log.Logger = new LoggerConfiguration()
                         .MinimumLevel.Information()
                         .WriteTo.File("logs/mismeapi-.log", rollingInterval: RollingInterval.Day)
                         .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureCors();
            services.ConfigureSignalR();
            services.ConfigureHangfire(Configuration);

            services.ConfigureDbContext(Configuration);
            services.ConfigureSwagger();
            services.ConfigureTokenAuth(Configuration);
            services.AddResponseCaching();
            services.ConfigureCompression();

            services.ConfigureDetection();
            services.AddHttpContextAccessor();
            services.AddMvc(config =>
            {
                config.Filters.Add(typeof(ApiValidationFilterAttribute));
                config.EnableEndpointRouting = false;
            }).AddNewtonsoftJson(opt => opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddTransient<IAmazonS3Service, AmazonS3Service>();
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IAccountService, AccountService>();
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

            var provider = services.BuildServiceProvider();
            var amazonS3Service = provider.GetService<IAmazonS3Service>();

            var apiMappings = new MappingProfiles(amazonS3Service);
            services.AddAutoMapper(x => x.AddProfile(apiMappings), typeof(Startup));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services, IBackgroundJobClient backgroundJobs, IRecurringJobManager recurringJobs)
        {
            app.UseCors();
            app.UseHangfireDashboard();
            var mismeBackJobs = services.GetRequiredService<IMismeBackgroundService>();
            recurringJobs.AddOrUpdate<IMismeBackgroundService>("ExpiredTokens", (e) => e.CleanExpiredTokensAsync(), "0 3 * * *");
            //recurringJobs.AddOrUpdate<IMismeBackgroundService>("Notifications", (e) => e.SendFireBaseNotificationsAsync(), "* * * * *");

            recurringJobs.AddOrUpdate<IMismeBackgroundService>("Notifications", (e) => e.SendFireBaseNotificationsRemindersAsync(), "0 18 * * *");

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
                endpoints.MapControllers();
            });
            CreateAdminUserAsync(services).Wait();
            //try
            //{
            //    ImportDishesAsync(services).Wait();
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            //RemoveDishesAsync(services).Wait();
        }

        private async Task CreateAdminUserAsync(IServiceProvider serviceProvider)
        {
            var _uow = serviceProvider.GetRequiredService<IUnitOfWork>();
            var admin = await _uow.UserRepository.FindBy(u => u.Email == "admin@mismedidas.com").FirstOrDefaultAsync();

            if (admin == null)
            {
                using (var hashAlgorithm = new SHA256CryptoServiceProvider())
                {
                    var byteValue = Encoding.UTF8.GetBytes("P@ssw0rd");
                    var byteHash = hashAlgorithm.ComputeHash(byteValue);

                    admin = new User()
                    {
                        FullName = "Mismedidas Admin",
                        Email = "admin@mismedidas.com",
                        Password = Convert.ToBase64String(byteHash),
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow,
                        Role = RoleEnum.ADMIN,
                        Status = StatusEnum.ACTIVE
                    };
                    await _uow.UserRepository.AddAsync(admin);
                    await _uow.CommitAsync();
                }
            }
        }

        //private async Task ImportDishesAsync(IServiceProvider serviceProvider)
        //{
        //    var _uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    using (var package = new ExcelPackage(new FileInfo("TablaAlimTransv2.xlsx")))
        //    {
        //        var sheetCount = package.Workbook.Worksheets.Count;
        //        var firstSheet = package.Workbook.Worksheets["Hoja1"];
        //        for (int i = 2; i <= 691; i++)
        //        //for (int i = 2; i <= 4; i++)
        //        {
        //            var dishName = firstSheet.Cells[i, 1].Text.Trim();
        //            var category = firstSheet.Cells[i, 2].Text.Trim();
        //            var calories = double.Parse(firstSheet.Cells[i, 8].Text.Trim());
        //            var carbohidrates = double.Parse(firstSheet.Cells[i, 9].Text.Trim());
        //            var proteins = double.Parse(firstSheet.Cells[i, 10].Text.Trim());
        //            var fat = double.Parse(firstSheet.Cells[i, 11].Text.Trim());
        //            var fiber = double.Parse(firstSheet.Cells[i, 12].Text.Trim());

        // var categoryBd = await _uow.TagRepository.GetAll().Where(t => t.Name ==
        // category).FirstOrDefaultAsync(); if (categoryBd == null) { categoryBd = new Tag();
        // categoryBd.Name = category; await _uow.TagRepository.AddAsync(categoryBd); await
        // _uow.CommitAsync(); } var tags = new List<DishTag>(); var dishTag = new DishTag();
        // dishTag.TagId = categoryBd.Id; dishTag.TaggedAt = DateTime.UtcNow; tags.Add(dishTag);

        //            var dish = new Dish();
        //            dish.Name = dishName;
        //            dish.Calories = calories;
        //            dish.Carbohydrates = carbohidrates;
        //            dish.Fat = fat;
        //            dish.Fiber = fiber;
        //            dish.Proteins = proteins;
        //            dish.DishTags = tags;
        //            await _uow.DishRepository.AddAsync(dish);
        //        }
        //        await _uow.CommitAsync();
        //    }
        //}

        //private async Task RemoveDishesAsync(IServiceProvider serviceProvider)
        //{
        //    var _uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        //    var fileServ = serviceProvider.GetRequiredService<IFileService>();

        // var dishes = _uow.DishRepository.GetAll(); foreach (var dish in dishes) { if
        // (!string.IsNullOrWhiteSpace(dish.Image)) { await fileServ.DeleteFileAsync(dish.Image); }
        // _uow.DishRepository.Delete(dish); }

        //    var tags = _uow.TagRepository.GetAll();
        //    foreach (var tag in tags)
        //    {
        //        _uow.TagRepository.Delete(tag);
        //    }
        //    await _uow.CommitAsync();
        //}
    }
}