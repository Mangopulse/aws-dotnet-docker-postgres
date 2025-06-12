using Microsoft.Extensions.Configuration;
using Shared.Data;
using Shared.Repositories;
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

// Add database context
builder.Services.AddSingleton<DatabaseContext>();

// Add services
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IStorageService, StorageService>();

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
