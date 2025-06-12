using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace E2E.Tests;

public class PostTests : TestBase
{
    private async Task<string> UploadTestImage()
    {
        // Create a test image file
        var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG header
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "test.png");

        var response = await UploadClient.PostAsync("/api/store/upload", content);
        response.EnsureSuccessStatusCode();
        var result = await DeserializeResponse<UploadResponse>(response);
        return result?.MediaId ?? throw new Exception("Failed to upload test image");
    }

    [Fact]
    public async Task CreatePost_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var mediaId = await UploadTestImage();
        var post = new
        {
            title = "Test Post",
            mediaId,
            jsonMeta = new { description = "Test description" }
        };

        // Act
        var response = await AdminApiClient.PostAsync("/api/admin/posts", CreateJsonContent(post));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await DeserializeResponse<PostResponse>(response);
        Assert.NotNull(result);
        Assert.Equal(post.title, result.Title);
        Assert.Equal(mediaId, result.MediaId);
    }

    [Fact]
    public async Task GetPost_WithValidId_ReturnsPost()
    {
        // Arrange
        var mediaId = await UploadTestImage();
        var post = new
        {
            title = "Test Post for Get",
            mediaId,
            jsonMeta = new { description = "Test description" }
        };

        var createResponse = await AdminApiClient.PostAsync("/api/admin/posts", CreateJsonContent(post));
        createResponse.EnsureSuccessStatusCode();
        var createdPost = await DeserializeResponse<PostResponse>(createResponse);

        // Act
        var response = await AdminApiClient.GetAsync($"/api/admin/posts/{createdPost.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<PostResponse>(response);
        Assert.NotNull(result);
        Assert.Equal(createdPost.Id, result.Id);
        Assert.Equal(post.title, result.Title);
    }

    [Fact]
    public async Task UpdatePost_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var mediaId = await UploadTestImage();
        var post = new
        {
            title = "Test Post for Update",
            mediaId,
            jsonMeta = new { description = "Test description" }
        };

        var createResponse = await AdminApiClient.PostAsync("/api/admin/posts", CreateJsonContent(post));
        createResponse.EnsureSuccessStatusCode();
        var createdPost = await DeserializeResponse<PostResponse>(createResponse);

        var update = new
        {
            title = "Updated Title",
            jsonMeta = new { description = "Updated description" }
        };

        // Act
        var response = await AdminApiClient.PutAsync($"/api/admin/posts/{createdPost.Id}", CreateJsonContent(update));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<PostResponse>(response);
        Assert.NotNull(result);
        Assert.Equal(update.title, result.Title);
    }

    [Fact]
    public async Task DeletePost_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var mediaId = await UploadTestImage();
        var post = new
        {
            title = "Test Post for Delete",
            mediaId,
            jsonMeta = new { description = "Test description" }
        };

        var createResponse = await AdminApiClient.PostAsync("/api/admin/posts", CreateJsonContent(post));
        createResponse.EnsureSuccessStatusCode();
        var createdPost = await DeserializeResponse<PostResponse>(createResponse);

        // Act
        var response = await AdminApiClient.DeleteAsync($"/api/admin/posts/{createdPost.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify post is deleted
        var getResponse = await AdminApiClient.GetAsync($"/api/admin/posts/{createdPost.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetPosts_ReturnsListOfPosts()
    {
        // Arrange
        var mediaId = await UploadTestImage();
        var post = new
        {
            title = "Test Post for List",
            mediaId,
            jsonMeta = new { description = "Test description" }
        };

        await AdminApiClient.PostAsync("/api/admin/posts", CreateJsonContent(post));

        // Act
        var response = await AdminApiClient.GetAsync("/api/admin/posts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<List<PostResponse>>(response);
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}

public class PostResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? MediaId { get; set; }
    public object? JsonMeta { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UploadResponse
{
    public string MediaId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
} 