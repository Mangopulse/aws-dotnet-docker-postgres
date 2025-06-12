using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace Shared.Storage
{
    public class AwsS3StorageProvider : IStorageProvider
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public AwsS3StorageProvider(IConfiguration configuration)
        {
            var awsOptions = configuration.GetSection("AWS").Get<AwsOptions>();
            _s3Client = new AmazonS3Client(
                awsOptions.AccessKey,
                awsOptions.SecretKey,
                new AmazonS3Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsOptions.Region)
                });
            _bucketName = awsOptions.S3.BucketName;
        }

        public async Task<UploadResult> UploadAsync(string container, string fileName, Stream content)
        {
            var key = $"{container}/{fileName}";
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = content,
                ContentType = GetContentType(fileName)
            };

            await _s3Client.PutObjectAsync(request);

            return new UploadResult
            {
                FileName = fileName,
                Url = await GetUrlAsync(container, fileName),
                Size = content.Length,
                ContentType = request.ContentType
            };
        }

        public async Task<Stream> DownloadAsync(string container, string fileName)
        {
            var key = $"{container}/{fileName}";
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            try
            {
                var response = await _s3Client.GetObjectAsync(request);
                var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new FileNotFoundException($"File {fileName} not found in container {container}", ex);
            }
        }

        public async Task DeleteAsync(string container, string fileName)
        {
            var key = $"{container}/{fileName}";
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request);
        }

        public async Task<string> GetUrlAsync(string container, string fileName)
        {
            var key = $"{container}/{fileName}";
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            return await Task.FromResult(_s3Client.GetPreSignedURL(request));
        }

        private string GetContentType(string fileName)
        {
            return Path.GetExtension(fileName).ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }

    public class AwsOptions
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public S3Options S3 { get; set; }
    }

    public class S3Options
    {
        public string BucketName { get; set; }
    }
} 