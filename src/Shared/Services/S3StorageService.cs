using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Interfaces;
using System.Text.RegularExpressions;

namespace Shared.Services;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<S3StorageService> _logger;
    private readonly string _bucketName;

    public S3StorageService(
        IAmazonS3 s3Client,
        IConfiguration configuration,
        ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _logger = logger;
        _bucketName = _configuration["AWS:BucketName"] ?? throw new ArgumentNullException("AWS:BucketName");
    }

    public string GetStorageProvider() => "s3";

    public async Task<StorageResult> UploadFileAsync(string fileName, Stream fileStream)
    {
        try
        {
            var validationResult = ValidateFile(fileName, fileStream.Length);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            var key = $"uploads/{Guid.NewGuid()}/{fileName}";
            using var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(fileStream, _bucketName, key);

            var url = $"https://{_bucketName}.s3.amazonaws.com/{key}";
            return new StorageResult { Success = true, Url = url };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} to S3", fileName);
            return new StorageResult { Success = false, Error = "Error uploading file to S3" };
        }
    }

    public async Task<StorageResult> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
            {
                return new StorageResult { Success = false, Error = "Invalid file URL" };
            }

            await _s3Client.DeleteObjectAsync(_bucketName, key);
            return new StorageResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileUrl} from S3", fileUrl);
            return new StorageResult { Success = false, Error = "Error deleting file from S3" };
        }
    }

    public async Task<StorageResult> GetFileUrlAsync(string filePath)
    {
        try
        {
            var key = filePath.StartsWith("uploads/") ? filePath : $"uploads/{filePath}";
            var url = $"https://{_bucketName}.s3.amazonaws.com/{key}";
            return new StorageResult { Success = true, Url = url };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file URL for {FilePath}", filePath);
            return new StorageResult { Success = false, Error = "Error getting file URL" };
        }
    }

    public StorageResult ValidateFile(string fileName, long fileSize)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return new StorageResult { Success = false, Error = "File name is required" };
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            return new StorageResult { Success = false, Error = "Invalid file type. Allowed types: JPG, JPEG, PNG, GIF, WebP" };
        }

        const int maxFileSize = 10 * 1024 * 1024; // 10MB
        if (fileSize > maxFileSize)
        {
            return new StorageResult { Success = false, Error = "File size exceeds the maximum limit of 10MB" };
        }

        return new StorageResult { Success = true };
    }

    private string? ExtractKeyFromUrl(string url)
    {
        var match = Regex.Match(url, $@"https://{_bucketName}\.s3\.amazonaws\.com/(.+)");
        return match.Success ? match.Groups[1].Value : null;
    }
} 