using Microsoft.AspNetCore.Http.Features;
using Amazon.S3;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

// Configure form options for file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
    options.ValueLengthLimit = 50 * 1024 * 1024; // 50MB
    options.MemoryBufferThreshold = 2 * 1024 * 1024; // 2MB
});

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

// Register AWS S3 service
builder.Services.AddSingleton<IAmazonS3>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var awsOptions = new AmazonS3Config
    {
        RegionEndpoint = Amazon.RegionEndpoint.USEast1 // Default region
    };
    return new AmazonS3Client(awsOptions);
});

// Register Azure Blob service
builder.Services.AddSingleton<BlobServiceClient>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("AzureStorage");
    return new BlobServiceClient(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
