using System.IO;
using System.Threading.Tasks;

namespace Shared.Interfaces;

public interface IS3Service
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteFileAsync(string filePath);
    string GetFileUrl(string filePath);
} 