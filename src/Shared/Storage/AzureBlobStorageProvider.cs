using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace Shared.Storage
{
    public class AzureBlobStorageProvider : IStorageProvider
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public AzureBlobStorageProvider(IConfiguration configuration)
        {
            var azureOptions = configuration.GetSection("Azure").Get<AzureOptions>();
            _blobServiceClient = new BlobServiceClient(azureOptions.ConnectionString);
            _containerName = azureOptions.BlobStorage.ContainerName;
        }

        public async Task<UploadResult> UploadAsync(string container, string fileName, Stream content)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(container);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);
            var contentType = GetContentType(fileName);

            await blobClient.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType });

            return new UploadResult
            {
                FileName = fileName,
                Url = await GetUrlAsync(container, fileName),
                Size = content.Length,
                ContentType = contentType
            };
        }

        public async Task<Stream> DownloadAsync(string container, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(container);
            var blobClient = containerClient.GetBlobClient(fileName);

            try
            {
                var response = await blobClient.DownloadAsync();
                var memoryStream = new MemoryStream();
                await response.Value.Content.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex) when (ex.Message.Contains("404"))
            {
                throw new FileNotFoundException($"File {fileName} not found in container {container}", ex);
            }
        }

        public async Task DeleteAsync(string container, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(container);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<string> GetUrlAsync(string container, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(container);
            var blobClient = containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
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

    public class AzureOptions
    {
        public string ConnectionString { get; set; }
        public BlobStorageOptions BlobStorage { get; set; }
    }

    public class BlobStorageOptions
    {
        public string ContainerName { get; set; }
    }
} 