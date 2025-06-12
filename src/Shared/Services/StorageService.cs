using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Interfaces;
using Shared.Services.Storage;

namespace Shared.Services;

public class StorageService : IStorageService
{
    private readonly IStorageProvider _storageProvider;
    private readonly string _storageProviderName;
    private readonly ILogger<StorageService> _logger;

    public StorageService(
        IConfiguration configuration,
        ILogger<StorageService> logger)
    {
        _storageProviderName = configuration["StorageProvider"] ?? "local";
        _logger = logger;

        _storageProvider = _storageProviderName.ToLower() switch
        {
            "s3" => new S3StorageProvider(
                new AmazonS3Client(
                    configuration["AWS:AccessKey"],
                    configuration["AWS:SecretKey"],
                    Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"] ?? "us-east-1")),
                configuration,
                logger),
            "azure" => new AzureStorageProvider(
                new Azure.Storage.Blobs.BlobServiceClient(configuration["Azure:Storage:ConnectionString"]),
                configuration,
                logger),
            _ => new LocalStorageProvider(configuration, logger)
        };
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file provided");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new ArgumentException("Invalid file type. Only JPEG, PNG, GIF, and WebP files are allowed.");
            }

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                throw new ArgumentException("File size exceeds 10MB limit");
            }

            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            return await _storageProvider.UploadFileAsync(file, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            throw;
        }
    }

    public async Task DeleteFileAsync(string filePath)
    {
        try
        {
            await _storageProvider.DeleteFileAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            throw;
        }
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        try
        {
            return await _storageProvider.GetFileAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file");
            throw;
        }
    }

    public string GetStorageProvider() => _storageProviderName;
} 