using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Shared.Storage
{
    public class LocalFileStorageProvider : IStorageProvider
    {
        private readonly string _basePath;

        public LocalFileStorageProvider(IConfiguration configuration)
        {
            var localOptions = configuration.GetSection("LocalStorage").Get<LocalStorageOptions>();
            _basePath = localOptions.BasePath;
            Directory.CreateDirectory(_basePath);
        }

        public async Task<UploadResult> UploadAsync(string container, string fileName, Stream content)
        {
            var containerPath = Path.Combine(_basePath, container);
            Directory.CreateDirectory(containerPath);

            var filePath = Path.Combine(containerPath, fileName);
            using (var fileStream = File.Create(filePath))
            {
                await content.CopyToAsync(fileStream);
            }

            return new UploadResult
            {
                FileName = fileName,
                Url = await GetUrlAsync(container, fileName),
                Size = content.Length,
                ContentType = GetContentType(fileName)
            };
        }

        public async Task<Stream> DownloadAsync(string container, string fileName)
        {
            var filePath = Path.Combine(_basePath, container, fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File {fileName} not found in container {container}");
            }

            var memoryStream = new MemoryStream();
            using (var fileStream = File.OpenRead(filePath))
            {
                await fileStream.CopyToAsync(memoryStream);
            }
            memoryStream.Position = 0;
            return memoryStream;
        }

        public Task DeleteAsync(string container, string fileName)
        {
            var filePath = Path.Combine(_basePath, container, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return Task.CompletedTask;
        }

        public Task<string> GetUrlAsync(string container, string fileName)
        {
            var filePath = Path.Combine(_basePath, container, fileName);
            return Task.FromResult($"file://{filePath}");
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

    public class LocalStorageOptions
    {
        public string BasePath { get; set; }
    }
} 