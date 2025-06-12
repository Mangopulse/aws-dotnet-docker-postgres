using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Shared.Interfaces;

namespace API.Services;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:BucketName"] ?? throw new ArgumentNullException("AWS:BucketName configuration is missing");
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var fileExtension = Path.GetExtension(fileName);
        var key = $"{Guid.NewGuid()}{fileExtension}";

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = fileStream,
            Key = key,
            BucketName = _bucketName,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead
        };

        var transferUtility = new TransferUtility(_s3Client);
        await transferUtility.UploadAsync(uploadRequest);

        return key;
    }

    public async Task DeleteFileAsync(string filePath)
    {
        var deleteRequest = new Amazon.S3.Model.DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = filePath
        };

        await _s3Client.DeleteObjectAsync(deleteRequest);
    }

    public string GetFileUrl(string filePath)
    {
        return $"https://{_bucketName}.s3.amazonaws.com/{filePath}";
    }
} 