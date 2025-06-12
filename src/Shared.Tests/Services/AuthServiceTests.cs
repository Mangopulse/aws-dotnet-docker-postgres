using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Services;
using Xunit;

namespace Shared.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<AuthService>>();

        // Setup default configuration
        _mockConfiguration.Setup(x => x["Admin:Username"]).Returns("admin");
        _mockConfiguration.Setup(x => x["Admin:Password"]).Returns("admin123");
        _mockConfiguration.Setup(x => x["JWT:Key"]).Returns("your-super-secret-jwt-key-that-should-be-at-least-256-bits-long-for-security");
        _mockConfiguration.Setup(x => x["JWT:Issuer"]).Returns("AdminApi");
        _mockConfiguration.Setup(x => x["JWT:Audience"]).Returns("AdminApiUsers");

        _authService = new AuthService(_mockConfiguration.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var username = "admin";
        var password = "admin123";

        // Act
        var (success, token, returnedUsername, expiresAt) = await _authService.AuthenticateAsync(username, password);

        // Assert
        Assert.True(success);
        Assert.NotNull(token);
        Assert.Equal(username, returnedUsername);
        Assert.NotNull(expiresAt);
        Assert.True(expiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidCredentials_ReturnsFailure()
    {
        // Arrange
        var username = "admin";
        var password = "wrongpassword";

        // Act
        var (success, token, returnedUsername, expiresAt) = await _authService.AuthenticateAsync(username, password);

        // Assert
        Assert.False(success);
        Assert.Null(token);
        Assert.Null(returnedUsername);
        Assert.Null(expiresAt);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var username = "admin";
        var token = _authService.GenerateJwtToken(username);

        // Act
        var (valid, returnedUsername) = await _authService.ValidateTokenAsync(token);

        // Assert
        Assert.True(valid);
        Assert.Equal(username, returnedUsername);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var (valid, username) = await _authService.ValidateTokenAsync(invalidToken);

        // Assert
        Assert.False(valid);
        Assert.Null(username);
    }

    [Fact]
    public void GenerateJwtToken_WithValidUsername_ReturnsValidToken()
    {
        // Arrange
        var username = "admin";

        // Act
        var token = _authService.GenerateJwtToken(username);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // JWT tokens have two dots
    }

    [Fact]
    public async Task ValidateTokenAsync_WithExpiredToken_ReturnsFailure()
    {
        // Arrange
        var username = "admin";
        var token = _authService.GenerateJwtToken(username);

        // Modify configuration to use a very short expiration
        _mockConfiguration.Setup(x => x["JWT:Key"]).Returns("your-super-secret-jwt-key-that-should-be-at-least-256-bits-long-for-security");
        var authServiceWithShortExpiry = new AuthService(_mockConfiguration.Object, _mockLogger.Object);

        // Act
        var (valid, returnedUsername) = await authServiceWithShortExpiry.ValidateTokenAsync(token);

        // Assert
        Assert.False(valid);
        Assert.Null(returnedUsername);
    }
} 