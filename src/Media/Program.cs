using Imageflow.Fluent;
using Imageflow.Bindings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add ImageFlow service
builder.Services.AddSingleton<ImageflowService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Serve static files from uploads directory
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthorization();
app.MapControllers();

app.Run();

public class ImageflowService
{
    public async Task<byte[]> ProcessImageAsync(byte[] inputBytes, int? width = null, int? height = null, string format = "jpeg", int quality = 90)
    {
        // For now, return original bytes. In production, implement proper ImageFlow processing
        return inputBytes;
    }

    public async Task<byte[]> CropImageAsync(byte[] inputBytes, int x, int y, int width, int height, string format = "jpeg", int quality = 90)
    {
        // For now, return original bytes. In production, implement proper ImageFlow processing
        return inputBytes;
    }
}
