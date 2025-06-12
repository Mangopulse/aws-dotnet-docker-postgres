using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Interfaces;
using Shared.Services;
using Xunit;

namespace Shared.Tests.Services;

public class StorageServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<StorageService>> _mockLogger;
    private readonly Mock<IStorageProvider> _mockStorageProvider;
    private readonly StorageService _storageService;

    public StorageServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<StorageService>>();
        _mockStorageProvider = new Mock<IStorageProvider>();

        // Setup default configuration
        _mockConfiguration.Setup(x => x["Storage:Provider"]).Returns("local");
        _mockConfiguration.Setup(x => x["Storage:Local:BasePath"]).Returns("uploads");

        _storageService = new StorageService(
            _mockConfiguration.Object,
            _mockLogger.Object,
            new[] { _mockStorageProvider.Object });
    }

    [Fact]
    public async Task UploadFileAsync_WithValidFile_ReturnsSuccess()
    {
        // Arrange
        var fileName = "test.jpg";
        var fileContent = new byte[] { 1, 2, 3 };
        var expectedUrl = "http://example.com/test.jpg";

        _mockStorageProvider.Setup(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>()))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _storageService.UploadFileAsync(fileName, new MemoryStream(fileContent));

        // Assert
        Assert.True(result.Success);
        Assert.Equal(expectedUrl, result.Url);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task UploadFileAsync_WithInvalidFile_ReturnsError()
    {
        // Arrange
        var fileName = "test.exe";
        var fileContent = new byte[] { 1, 2, 3 };

        // Act
        var result = await _storageService.UploadFileAsync(fileName, new MemoryStream(fileContent));

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Url);
        Assert.NotNull(result.Error);
        Assert.Contains("Invalid file type", result.Error);
    }

    [Fact]
    public async Task DeleteFileAsync_WithValidUrl_ReturnsSuccess()
    {
        // Arrange
        var fileUrl = "http://example.com/test.jpg";

        _mockStorageProvider.Setup(x => x.DeleteFileAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _storageService.DeleteFileAsync(fileUrl);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task DeleteFileAsync_WithInvalidUrl_ReturnsError()
    {
        // Arrange
        var fileUrl = "invalid-url";

        // Act
        var result = await _storageService.DeleteFileAsync(fileUrl);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Contains("Invalid file URL", result.Error);
    }

    [Fact]
    public async Task GetFileUrlAsync_WithValidPath_ReturnsUrl()
    {
        // Arrange
        var filePath = "test.jpg";
        var expectedUrl = "http://example.com/test.jpg";

        _mockStorageProvider.Setup(x => x.GetFileUrlAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _storageService.GetFileUrlAsync(filePath);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(expectedUrl, result.Url);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task GetFileUrlAsync_WithInvalidPath_ReturnsError()
    {
        // Arrange
        var filePath = "";

        // Act
        var result = await _storageService.GetFileUrlAsync(filePath);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Url);
        Assert.NotNull(result.Error);
        Assert.Contains("Invalid file path", result.Error);
    }

    [Fact]
    public void ValidateFile_WithValidFile_ReturnsTrue()
    {
        // Arrange
        var fileName = "test.jpg";
        var fileContent = new byte[] { 1, 2, 3 };

        // Act
        var result = _storageService.ValidateFile(fileName, fileContent.Length);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Error);
    }

    [Fact]
    public void ValidateFile_WithInvalidExtension_ReturnsFalse()
    {
        // Arrange
        var fileName = "test.exe";
        var fileContent = new byte[] { 1, 2, 3 };

        // Act
        var result = _storageService.ValidateFile(fileName, fileContent.Length);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Contains("Invalid file type", result.Error);
    }

    [Fact]
    public void ValidateFile_WithFileTooLarge_ReturnsFalse()
    {
        // Arrange
        var fileName = "test.jpg";
        var fileSize = 11 * 1024 * 1024; // 11MB

        // Act
        var result = _storageService.ValidateFile(fileName, fileSize);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Contains("File size exceeds", result.Error);
    }
} 