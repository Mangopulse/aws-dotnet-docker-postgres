using Microsoft.AspNetCore.Mvc;
using Amazon.S3;
using Amazon.S3.Model;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.ComponentModel.DataAnnotations;

namespace Upload.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoreController : ControllerBase
{
    private readonly ILogger<StoreController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAmazonS3 _s3Client;
    private readonly BlobServiceClient _blobServiceClient;

    public StoreController(
        ILogger<StoreController> logger,
        IConfiguration configuration,
        IAmazonS3 s3Client,
        BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _configuration = configuration;
        _s3Client = s3Client;
        _blobServiceClient = blobServiceClient;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadPhoto([Required] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Invalid file type. Only JPEG, PNG, GIF, and WebP files are allowed.");
            }

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest("File size exceeds 10MB limit");
            }

            // Get storage provider from configuration
            var storageProvider = _configuration["StorageProvider"] ?? "local";
            var fileName = $"{Guid.NewGuid()}{fileExtension}";

            string fileUrl;

            switch (storageProvider.ToLower())
            {
                case "s3":
                    fileUrl = await UploadToS3(file, fileName);
                    break;
                case "azure":
                    fileUrl = await UploadToAzure(file, fileName);
                    break;
                case "local":
                default:
                    fileUrl = await UploadToLocal(file, fileName);
                    break;
            }

            _logger.LogInformation("File uploaded successfully: {FileName} to {Provider}", fileName, storageProvider);

            return Ok(new
            {
                FileName = fileName,
                OriginalFileName = file.FileName,
                FileUrl = fileUrl,
                Size = file.Length,
                StorageProvider = storageProvider,
                UploadedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, "An error occurred while uploading the file");
        }
    }

    private async Task<string> UploadToS3(IFormFile file, string fileName)
    {
        var bucketName = _configuration["AWS:S3:BucketName"] ?? "your-bucket-name";
        var keyName = $"uploads/{fileName}";

        using var stream = file.OpenReadStream();
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = keyName,
            InputStream = stream,
            ContentType = file.ContentType,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
        };

        await _s3Client.PutObjectAsync(request);

        var region = _configuration["AWS:Region"] ?? "us-east-1";
        return $"https://{bucketName}.s3.{region}.amazonaws.com/{keyName}";
    }

    private async Task<string> UploadToAzure(IFormFile file, string fileName)
    {
        var containerName = _configuration["Azure:Storage:ContainerName"] ?? "uploads";
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        
        await containerClient.CreateIfNotExistsAsync();
        var blobClient = containerClient.GetBlobClient(fileName);

        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

        return blobClient.Uri.ToString();
    }

    private async Task<string> UploadToLocal(IFormFile file, string fileName)
    {
        var uploadsFolder = _configuration["LocalStorage:Path"] ?? "uploads";
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), uploadsFolder);
        
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }

        var filePath = Path.Combine(uploadsPath, fileName);
        
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5002";
        return $"{baseUrl}/{uploadsFolder}/{fileName}";
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        var storageProvider = _configuration["StorageProvider"] ?? "local";
        return Ok(new
        {
            Service = "Upload Service",
            Status = "Healthy",
            StorageProvider = storageProvider,
            Timestamp = DateTime.UtcNow
        });
    }
} 