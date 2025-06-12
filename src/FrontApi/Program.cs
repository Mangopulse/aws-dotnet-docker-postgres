using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using API.Data;
using API.Repositories;
using Shared.Interfaces;
using Shared.Services;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from config.json in repository root
builder.Configuration.AddJsonFile(Path.Combine("..", "..", "config.json"), optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IStorageService, S3StorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
