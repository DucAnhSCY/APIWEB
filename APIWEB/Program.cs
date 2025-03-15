using APIWEB.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendApp", // T�n c?a CORS policy (b?n c� th? ??t t�n kh�c)
        policy =>
        {
            policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500") // **Quan tr?ng:** Thay th? b?ng ngu?n g?c (origin) frontend c?a b?n. N?u b?n d�ng Live Server VS Code, ?�y th??ng l� c�c gi� tr? n�y.
                   .AllowAnyMethod() // Cho ph�p t?t c? c�c HTTP methods (GET, POST, PUT, DELETE...) - **Ch? d�ng cho ph�t tri?n**.
                   .AllowAnyHeader(); // Cho ph�p t?t c? c�c HTTP headers - **Ch? d�ng cho ph�t tri?n**.
        });
});
builder.Services.AddControllers();
builder.Services.AddDbContext<DBContextTest>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBContextTest")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors("AllowFrontendApp");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
