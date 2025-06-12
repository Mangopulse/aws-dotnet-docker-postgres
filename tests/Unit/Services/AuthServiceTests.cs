using Microsoft.Extensions.Configuration;
using Moq;
using Shared.Models;
using Shared.Services;
using Xunit;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;

namespace Unit.Tests.Services;

public class AuthServiceTests
{
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"JWT:Key", "test_secret_key_12345678901234567890"},
            {"JWT:Issuer", "DockerX-Test"},
            {"JWT:Audience", "DockerX-Test"},
            {"JWT:ExpirationInHours", "24"},
            {"Admin:Username", "admin"},
            {"Admin:Password", "admin123"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        _authService = new AuthService(configuration);
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
        var (_, token, _, _) = await _authService.AuthenticateAsync(username, "admin123");

        // Act
        var (valid, returnedUsername) = await _authService.ValidateTokenAsync(token!);

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
    public async Task ValidateTokenAsync_WithExpiredToken_ReturnsFailure()
    {
        // Arrange
        var username = "admin";
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("your-256-bit-secret-key-here-minimum-32-characters");
        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", username) }),
            NotBefore = now.AddMinutes(-5),
            Expires = now.AddMinutes(-1), // Token that expired 1 minute ago
            Issuer = "your-issuer",
            Audience = "your-audience",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var expiredToken = tokenHandler.WriteToken(token);

        // Act
        var (valid, returnedUsername) = await _authService.ValidateTokenAsync(expiredToken);

        // Assert
        Assert.False(valid);
        Assert.Null(returnedUsername);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-token")]
    public async Task ValidateTokenAsync_WithInvalidTokens_ReturnsFailure(string token)
    {
        // Act
        var (valid, username) = await _authService.ValidateTokenAsync(token);

        // Assert
        Assert.False(valid);
        Assert.Null(username);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithNullToken_ReturnsFailure()
    {
        // Act
        var (valid, username) = await _authService.ValidateTokenAsync(null!);

        // Assert
        Assert.False(valid);
        Assert.Null(username);
    }
} 