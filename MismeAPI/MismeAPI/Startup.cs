using AutoMapper;
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
using MismeAPI.Service.Impls;
using MismeAPI.Services;
using MismeAPI.Services.Impls;
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
            services.AddMvc(config =>
            {
                config.Filters.Add(typeof(ApiValidationFilterAttribute));
                config.EnableEndpointRouting = false;
            }).AddNewtonsoftJson(opt => opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            services.ConfigureDbContext(Configuration);
            services.ConfigureSwagger();
            services.ConfigureTokenAuth(Configuration);
            services.ConfigureCompression();
            services.ConfigureCors();
            services.ConfigureDetection();

            services.AddHttpContextAccessor();
            services.AddAutoMapper(typeof(Startup));

            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddTransient<IPersonalDataService, PersonalDataService>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IEmailService, EmailService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services)
        {
            app.UseCors(builder => builder
           .AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Misme API V1");
                });
            }
            else
            {
                app.UseHsts();
            }
            CreateAdminUserAsync(services).Wait();
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware(typeof(ErrorWrappingMiddleware));
            app.UseResponseCompression();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
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
                        Status = StatusEnum.ACTIVE,
                        Username = "mismedidas"
                    };
                    await _uow.UserRepository.AddAsync(admin);
                    await _uow.CommitAsync();
                }
            }
        }
    }
}