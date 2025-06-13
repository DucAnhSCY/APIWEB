using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BCrypt.Net;
using diendan2.Models2;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System;
using System.Linq;

namespace diendan2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ✅ Add S3 Service
            var config = builder.Configuration.GetSection("DigitalOcean");
            var accessKey = config["AccessKey"];
            var secretKey = config["SecretKey"];
            var endpoint = config["Endpoint"];

            var s3Config = new AmazonS3Config
            {
                ServiceURL = endpoint,
                ForcePathStyle = true, // Ensure this is set to true for DigitalOcean Spaces
                UseHttp = false,
                RegionEndpoint = Amazon.RegionEndpoint.USEast1 // Update to your region
            };

            builder.Services.AddSingleton<IAmazonS3>(new AmazonS3Client(accessKey, secretKey, s3Config));

            // ✅ Add Database Context
            builder.Services.AddDbContext<DBContextTest2>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));            // ✅ Enable CORS (Cross-Origin Resource Sharing)
            builder.Services.AddCors(options =>
            {                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins(
                        "http://127.0.0.1:5502", "http://localhost:5502", 
                        "http://127.0.0.1:5500", "http://localhost:5500",
                        "http://127.0.0.1:81", "http://localhost:81")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // ✅ Add Authentication (JWT)
            var jwtSecretKey = builder.Configuration["Jwt:Key"] ?? "your_secret_key_here";
            var key = Encoding.UTF8.GetBytes(jwtSecretKey);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"]
                    };
                });

            // ✅ Add Authorization
            builder.Services.AddAuthorization();

            // ✅ Add Controllers
            builder.Services.AddControllers();

            // ✅ Add Swagger with JWT Support
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "diendan API", Version = "v1" });

                // 🔹 Enable Authorization in Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer {your JWT token}'"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new string[] {}
                    }
                });
            });

            // Add DigitalOcean Spaces Service
            builder.Services.AddSingleton<Services.DigitalOceanSpacesService>();

            var app = builder.Build();

            // ✅ Automatically create an Admin if none exists
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DBContextTest2>(); // Update to use DBContextTest2
                context.Database.Migrate(); // Ensure database is up to date

                if (!context.Users.Any(u => u.Role == "Admin"))
                {
                    var admin = new User
                    {
                        Username = "admin",
                        Email = "admin@gmail.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("123"), // Change this to a strong password
                        Role = "Admin",
                        Status = "active",
                        JoinDate = DateTime.UtcNow
                    };

                    context.Users.Add(admin);
                    context.SaveChanges();
                }
            }            // ✅ Enable Swagger UI in Development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // ✅ Enable CORS (MUST BE BEFORE Authentication)
            app.UseCors("AllowAll");

            // ✅ Enable Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // ✅ Serve Static Files
            app.UseStaticFiles();

            // Configure additional static file middleware for uploads directory
            string contentRootPath = builder.Environment.ContentRootPath;
            string uploadsPath = Path.Combine(contentRootPath, "wwwroot", "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadsPath),
                RequestPath = "/uploads"
            });

            // ✅ Map Controllers
            app.MapControllers();

            // ✅ Map Default Route
            app.MapFallbackToFile("index.html");
            app.Run();
        }
    }
}
