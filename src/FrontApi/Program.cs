using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using API.Data;
using API.Repositories;
using Shared.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from config.json in repository root
builder.Configuration.AddJsonFile(Path.Combine("..", "..", "config.json"), optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add repositories
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IMediaRepository, MediaRepository>();

// Add controllers
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001") // Next.js dev servers
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

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

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
