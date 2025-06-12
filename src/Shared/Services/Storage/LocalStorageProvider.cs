using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Interfaces;

namespace Shared.Services.Storage;

public class LocalStorageProvider : IStorageProvider
{
    private readonly string _uploadsPath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalStorageProvider> _logger;

    public LocalStorageProvider(
        IConfiguration configuration,
        ILogger<LocalStorageProvider> logger)
    {
        var uploadsFolder = configuration["LocalStorage:Path"] ?? "uploads";
        _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), uploadsFolder);
        _baseUrl = configuration["BaseUrl"] ?? "http://localhost:5002";
        _logger = logger;

        if (!Directory.Exists(_uploadsPath))
        {
            Directory.CreateDirectory(_uploadsPath);
        }
    }

    public async Task<string> UploadFileAsync(IFormFile file, string fileName)
    {
        try
        {
            var filePath = Path.Combine(_uploadsPath, fileName);
            
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"{_baseUrl}/uploads/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to local storage");
            throw;
        }
    }

    public async Task DeleteFileAsync(string filePath)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            var fullPath = Path.Combine(_uploadsPath, fileName);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from local storage");
            throw;
        }
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            var fullPath = Path.Combine(_uploadsPath, fileName);
            
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {fileName}");
            }

            return await File.ReadAllBytesAsync(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file from local storage");
            throw;
        }
    }
} 