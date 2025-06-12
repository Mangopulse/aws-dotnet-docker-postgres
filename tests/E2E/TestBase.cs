using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace E2E.Tests;

public abstract class TestBase : IAsyncLifetime
{
    protected readonly WebApplicationFactory<AdminApi.Program> AdminApiFactory;
    protected readonly WebApplicationFactory<FrontApi.Program> FrontApiFactory;
    protected readonly WebApplicationFactory<Upload.Program> UploadFactory;
    protected readonly HttpClient AdminApiClient;
    protected readonly HttpClient FrontApiClient;
    protected readonly HttpClient UploadClient;
    protected string? AdminToken;

    protected TestBase()
    {
        AdminApiFactory = new WebApplicationFactory<AdminApi.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Add any test-specific service configuration here
                });
            });

        FrontApiFactory = new WebApplicationFactory<FrontApi.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
            });

        UploadFactory = new WebApplicationFactory<Upload.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
            });

        AdminApiClient = AdminApiFactory.CreateClient();
        FrontApiClient = FrontApiFactory.CreateClient();
        UploadClient = UploadFactory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Login as admin and get token
        var loginResponse = await AdminApiClient.PostAsync("/api/auth/login", new StringContent(
            JsonSerializer.Serialize(new { username = "admin", password = "admin123" }),
            Encoding.UTF8,
            "application/json"
        ));

        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        AdminToken = loginResult?.Token;

        if (AdminToken != null)
        {
            AdminApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
        }
    }

    public async Task DisposeAsync()
    {
        AdminApiClient.Dispose();
        FrontApiClient.Dispose();
        UploadClient.Dispose();
        await AdminApiFactory.DisposeAsync();
        await FrontApiFactory.DisposeAsync();
        await UploadFactory.DisposeAsync();
    }

    protected async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    protected StringContent CreateJsonContent<T>(T data)
    {
        return new StringContent(
            JsonSerializer.Serialize(data),
            Encoding.UTF8,
            "application/json"
        );
    }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
} 