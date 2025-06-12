using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace E2E.Tests;

public class PublicApiTests : TestBase
{
    private async Task<string> CreateTestPost()
    {
        // Upload test image
        var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG header
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "test.png");

        var uploadResponse = await UploadClient.PostAsync("/api/store/upload", content);
        uploadResponse.EnsureSuccessStatusCode();
        var uploadResult = await DeserializeResponse<UploadResponse>(uploadResponse);

        // Create post with the uploaded image
        var post = new
        {
            title = "Public Test Post",
            mediaId = uploadResult.MediaId,
            jsonMeta = new { description = "Test description" }
        };

        var createResponse = await AdminApiClient.PostAsync("/api/admin/posts", CreateJsonContent(post));
        createResponse.EnsureSuccessStatusCode();
        var createdPost = await DeserializeResponse<PostResponse>(createResponse);
        return createdPost.Id;
    }

    [Fact]
    public async Task GetPublicPosts_ReturnsListOfPosts()
    {
        // Arrange
        await CreateTestPost();

        // Act
        var response = await FrontApiClient.GetAsync("/api/posts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<List<PublicPostResponse>>(response);
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetPublicPost_WithValidId_ReturnsPost()
    {
        // Arrange
        var postId = await CreateTestPost();

        // Act
        var response = await FrontApiClient.GetAsync($"/api/posts/{postId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<PublicPostResponse>(response);
        Assert.NotNull(result);
        Assert.Equal(postId, result.Id);
    }

    [Fact]
    public async Task GetPublicPost_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await FrontApiClient.GetAsync("/api/posts/invalid-id");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicPosts_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await CreateTestPost();
        await CreateTestPost();

        // Act
        var response = await FrontApiClient.GetAsync("/api/posts/paged?page=1&pageSize=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeResponse<PagedResponse<PublicPostResponse>>(response);
        Assert.NotNull(result);
        Assert.Equal(1, result.Items.Count);
        Assert.True(result.TotalCount > 1);
    }
}

public class PublicPostResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public object? JsonMeta { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
} 