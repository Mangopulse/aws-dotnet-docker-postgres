using System.Net;
using Xunit;

namespace E2E.Tests;

public class AuthTests : TestBase
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var credentials = new { username = "admin", password = "admin123" };

        // Act
        var response = await AdminApiClient.PostAsync("/api/auth/login", CreateJsonContent(credentials));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<LoginResponse>(response);
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.ExpiresAt);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var credentials = new { username = "admin", password = "wrongpassword" };

        // Act
        var response = await AdminApiClient.PostAsync("/api/auth/login", CreateJsonContent(credentials));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = AdminApiFactory.CreateClient(); // Create new client without token

        // Act
        var response = await client.GetAsync("/api/admin/posts");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_ReturnsSuccess()
    {
        // Act
        var response = await AdminApiClient.GetAsync("/api/admin/posts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
} 