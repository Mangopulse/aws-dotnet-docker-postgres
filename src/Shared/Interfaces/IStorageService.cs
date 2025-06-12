using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Shared.Interfaces;

public interface IStorageService
{
    Task<StorageResult> UploadFileAsync(string fileName, Stream fileStream);
    Task<StorageResult> DeleteFileAsync(string fileUrl);
    Task<StorageResult> GetFileUrlAsync(string filePath);
    StorageResult ValidateFile(string fileName, long fileSize);
    string GetStorageProvider();
}

public class StorageResult
{
    public bool Success { get; set; }
    public string? Url { get; set; }
    public string? Error { get; set; }
} 