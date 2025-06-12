using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Interfaces;

namespace Shared.Services.Storage;

public class S3StorageProvider : IStorageProvider
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _region;
    private readonly ILogger<S3StorageProvider> _logger;

    public S3StorageProvider(
        IAmazonS3 s3Client,
        IConfiguration configuration,
        ILogger<S3StorageProvider> logger)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:S3:BucketName"] ?? throw new ArgumentNullException("AWS:S3:BucketName");
        _region = configuration["AWS:Region"] ?? "us-east-1";
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string fileName)
    {
        try
        {
            var keyName = $"uploads/{fileName}";

            using var stream = file.OpenReadStream();
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = keyName,
                InputStream = stream,
                ContentType = file.ContentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            await _s3Client.PutObjectAsync(request);
            return $"https://{_bucketName}.s3.{_region}.amazonaws.com/{keyName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to S3");
            throw;
        }
    }

    public async Task DeleteFileAsync(string filePath)
    {
        try
        {
            var key = Path.GetFileName(filePath);
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = $"uploads/{key}"
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from S3");
            throw;
        }
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        try
        {
            var key = Path.GetFileName(filePath);
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = $"uploads/{key}"
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var responseStream = response.ResponseStream;
            using var memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file from S3");
            throw;
        }
    }
} 