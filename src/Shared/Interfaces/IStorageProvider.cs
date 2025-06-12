using Microsoft.AspNetCore.Http;

namespace Shared.Interfaces;

public interface IStorageProvider
{
    Task<string> UploadFileAsync(IFormFile file, string fileName);
    Task DeleteFileAsync(string filePath);
    Task<byte[]> GetFileAsync(string filePath);
}

public interface IStorageService
{
    Task<string> UploadFileAsync(IFormFile file);
    Task DeleteFileAsync(string filePath);
    Task<byte[]> GetFileAsync(string filePath);
    string GetStorageProvider();
} 