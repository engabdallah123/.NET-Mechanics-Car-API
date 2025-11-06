
using Data;
using Domain.DTO.Account;
using Domain.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using Services;
using Services.IReposetory;
using Services.Repository;
using System;
using System.Text;

namespace CarMaintenance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<DataContext>(op =>
            {
                op.UseSqlServer(builder.Configuration.GetConnectionString("Connection1")).UseLazyLoadingProxies();
            });

            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 4;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
              .AddEntityFrameworkStores<DataContext>()
              .AddDefaultTokenProviders();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            builder.Services.AddMemoryCache();
            builder.Services.AddHostedService<WorkOrderStatusScheduler>();
            builder.Services.AddScoped<ICustomerRepo,CustomerRepo>();
            builder.Services.AddScoped<IVehicleRepo,VehicleRepo>();
            builder.Services.AddScoped<IVehicleModelRepo,VehicleModelRepo>();
            builder.Services.AddScoped<IRepairTaskRepo,RepairTaskRepo>();
            builder.Services.AddScoped<IPartRepo,PartRepo>();
            builder.Services.AddScoped<IScheduleRepo, ScheduleRepo>();
            builder.Services.AddScoped<IWorkOrderRepo, WorkOrderRepo>();
            builder.Services.AddScoped<IInvoiceRepo, InvoiceRepo>();

           

            var jwtSection = builder.Configuration.GetSection("JWT").Get<JWTOption>();
              builder.Services.AddSingleton(jwtSection);
              builder.Services.AddAuthentication(op => op.DefaultAuthenticateScheme = "my scheme")
                .AddJwtBearer("my scheme", options =>
                {
                    options.SaveToken = true;
                    string secretKey = jwtSection.SecretKey;
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        IssuerSigningKey = key,
                    };
                });

            QuestPDF.Settings.License = LicenseType.Community;

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(c => c.SwaggerEndpoint(url: "/openapi/v1.json", "v1"));
            }


            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
