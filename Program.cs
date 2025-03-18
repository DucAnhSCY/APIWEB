using diendan; // Add this using directive
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

var builder = WebApplication.CreateBuilder(args);

// ✅ Add Database Context
builder.Services.AddDbContext<DBContextTest2>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Enable CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ✅ Add Authentication (JWT)
var jwtSecretKey = builder.Configuration["JwtSettings:Secret"] ?? "your_secret_key_here";
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
            ValidateIssuer = false,
            ValidateAudience = false
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
            Password = BCrypt.Net.BCrypt.HashPassword("1"), // Change this to a strong password
            Role = "Admin",
            Status = "active",
            JoinDate = DateTime.UtcNow
        };

        context.Users.Add(admin);
        context.SaveChanges();
    }
}

// ✅ Enable Swagger UI in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Enable CORS (NO RESTRICTIONS)
app.UseCors("AllowAll");

// ✅ Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// ✅ Serve Static Files
app.UseStaticFiles();

// ✅ Map Controllers
app.MapControllers();

// ✅ Map Default Route
app.MapFallbackToFile("index.html");

app.Run();