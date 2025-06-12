using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Upload.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoreController : ControllerBase
{
    private readonly ILogger<StoreController> _logger;
    private readonly IStorageService _storageService;

    public StoreController(
        ILogger<StoreController> logger,
        IStorageService storageService)
    {
        _logger = logger;
        _storageService = storageService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadPhoto([Required] IFormFile file)
    {
        try
        {
            var fileUrl = await _storageService.UploadFileAsync(file);

            _logger.LogInformation("File uploaded successfully: {FileName}", file.FileName);

            return Ok(new
            {
                FileName = Path.GetFileName(fileUrl),
                OriginalFileName = file.FileName,
                FileUrl = fileUrl,
                Size = file.Length,
                //StorageProvider = _storageService.GetStorageProvider(),
                UploadedAt = DateTime.UtcNow
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid file upload attempt");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, "An error occurred while uploading the file");
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            Service = "Upload Service",
            Status = "Healthy",
            //StorageProvider = _storageService.GetStorageProvider(),
            Timestamp = DateTime.UtcNow
        });
    }
} 