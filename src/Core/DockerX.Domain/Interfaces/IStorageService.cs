using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Shared.Interfaces;

public interface IStorageService
{
    Task<string> UploadFileAsync(IFormFile file);
    Task DeleteFileAsync(string fileUrl);
}

public class StorageResult
{
    public bool Success { get; set; }
    public string? Url { get; set; }
    public string? Error { get; set; }
} 