using Microsoft.AspNetCore.Mvc;

namespace Media.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly ILogger<MediaController> _logger;
    private readonly ImageflowService _imageflowService;
    private readonly IConfiguration _configuration;

    public MediaController(ILogger<MediaController> logger, ImageflowService imageflowService, IConfiguration configuration)
    {
        _logger = logger;
        _imageflowService = imageflowService;
        _configuration = configuration;
    }

    [HttpGet("image/{fileName}")]
    public async Task<IActionResult> GetImage(string fileName, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string format = "jpeg", [FromQuery] int quality = 90)
    {
        try
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            var filePath = Path.Combine(uploadsPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Image not found");
            }

            var inputBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            // Process image if width or height specified
            byte[] outputBytes;
            if (width.HasValue || height.HasValue)
            {
                outputBytes = await _imageflowService.ProcessImageAsync(inputBytes, width, height, format, quality);
            }
            else
            {
                outputBytes = inputBytes;
            }

            var contentType = format.ToLower() switch
            {
                "png" => "image/png",
                "gif" => "image/gif",
                "webp" => "image/webp",
                _ => "image/jpeg"
            };

            return File(outputBytes, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image {FileName}", fileName);
            return StatusCode(500, "Error processing image");
        }
    }

    [HttpGet("crop/{fileName}")]
    public async Task<IActionResult> CropImage(
        string fileName,
        [FromQuery] int x,
        [FromQuery] int y,
        [FromQuery] int width,
        [FromQuery] int height,
        [FromQuery] string format = "jpeg",
        [FromQuery] int quality = 90)
    {
        try
        {
            if (width <= 0 || height <= 0 || x < 0 || y < 0)
            {
                return BadRequest("Invalid crop parameters");
            }

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            var filePath = Path.Combine(uploadsPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Image not found");
            }

            var inputBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var outputBytes = await _imageflowService.CropImageAsync(inputBytes, x, y, width, height, format, quality);

            var contentType = format.ToLower() switch
            {
                "png" => "image/png",
                "gif" => "image/gif",
                "webp" => "image/webp",
                _ => "image/jpeg"
            };

            return File(outputBytes, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cropping image {FileName}", fileName);
            return StatusCode(500, "Error cropping image");
        }
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessUploadedImage([FromForm] IFormFile file, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string format = "jpeg", [FromQuery] int quality = 90)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Invalid file type");
            }

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var inputBytes = stream.ToArray();

            var outputBytes = await _imageflowService.ProcessImageAsync(inputBytes, width, height, format, quality);

            var contentType = format.ToLower() switch
            {
                "png" => "image/png",
                "gif" => "image/gif",
                "webp" => "image/webp",
                _ => "image/jpeg"
            };

            return File(outputBytes, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing uploaded image");
            return StatusCode(500, "Error processing image");
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            Service = "Media Processing Service",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            SupportedFormats = new[] { "jpeg", "png", "gif", "webp" },
            Features = new[] { "resize", "crop", "format_conversion", "quality_adjustment" }
        });
    }
} 