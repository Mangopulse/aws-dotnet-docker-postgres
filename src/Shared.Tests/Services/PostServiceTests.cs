using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Interfaces;
using Shared.Models;
using Shared.Services;
using Xunit;

namespace Shared.Tests.Services;

public class PostServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<PostService>> _mockLogger;
    private readonly Mock<IPostRepository> _mockPostRepository;
    private readonly Mock<IStorageService> _mockStorageService;
    private readonly PostService _postService;

    public PostServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<PostService>>();
        _mockPostRepository = new Mock<IPostRepository>();
        _mockStorageService = new Mock<IStorageService>();

        _postService = new PostService(
            _mockConfiguration.Object,
            _mockLogger.Object,
            _mockPostRepository.Object,
            _mockStorageService.Object);
    }

    [Fact]
    public async Task GetPostsAsync_ReturnsAllPosts()
    {
        // Arrange
        var expectedPosts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), Title = "Test Post 1", PublicId = 1 },
            new() { Id = Guid.NewGuid(), Title = "Test Post 2", PublicId = 2 }
        };

        _mockPostRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(expectedPosts);

        // Act
        var result = await _postService.GetPostsAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(expectedPosts.Count, result.Posts?.Count);
        Assert.Equal(expectedPosts[0].Title, result.Posts?[0].Title);
        Assert.Equal(expectedPosts[1].Title, result.Posts?[1].Title);
    }

    [Fact]
    public async Task GetPostAsync_WithValidId_ReturnsPost()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var expectedPost = new Post { Id = postId, Title = "Test Post", PublicId = 1 };

        _mockPostRepository.Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync(expectedPost);

        // Act
        var result = await _postService.GetPostAsync(postId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Post);
        Assert.Equal(expectedPost.Title, result.Post.Title);
    }

    [Fact]
    public async Task GetPostAsync_WithInvalidId_ReturnsError()
    {
        // Arrange
        var postId = Guid.NewGuid();

        _mockPostRepository.Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync((Post?)null);

        // Act
        var result = await _postService.GetPostAsync(postId);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Post);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task CreatePostAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var post = new Post { Title = "New Post" };
        var mediaFile = new MemoryStream(new byte[] { 1, 2, 3 });
        var expectedUrl = "http://example.com/test.jpg";

        _mockStorageService.Setup(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>()))
            .ReturnsAsync(new StorageResult { Success = true, Url = expectedUrl });

        _mockPostRepository.Setup(x => x.CreateAsync(It.IsAny<Post>()))
            .ReturnsAsync(new Post { Id = Guid.NewGuid(), Title = post.Title, MediaUrl = expectedUrl });

        // Act
        var result = await _postService.CreatePostAsync(post, "test.jpg", mediaFile);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Post);
        Assert.Equal(post.Title, result.Post.Title);
        Assert.Equal(expectedUrl, result.Post.MediaUrl);
    }

    [Fact]
    public async Task CreatePostAsync_WithInvalidFile_ReturnsError()
    {
        // Arrange
        var post = new Post { Title = "New Post" };
        var mediaFile = new MemoryStream(new byte[] { 1, 2, 3 });

        _mockStorageService.Setup(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>()))
            .ReturnsAsync(new StorageResult { Success = false, Error = "Invalid file" });

        // Act
        var result = await _postService.CreatePostAsync(post, "test.exe", mediaFile);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Post);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task UpdatePostAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = new Post { Id = postId, Title = "Updated Post" };

        _mockPostRepository.Setup(x => x.UpdateAsync(It.IsAny<Post>()))
            .ReturnsAsync(post);

        // Act
        var result = await _postService.UpdatePostAsync(postId, post);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Post);
        Assert.Equal(post.Title, result.Post.Title);
    }

    [Fact]
    public async Task UpdatePostAsync_WithInvalidId_ReturnsError()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = new Post { Id = Guid.NewGuid(), Title = "Updated Post" };

        // Act
        var result = await _postService.UpdatePostAsync(postId, post);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Post);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task DeletePostAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = new Post { Id = postId, Title = "Test Post", MediaUrl = "http://example.com/test.jpg" };

        _mockPostRepository.Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync(post);

        _mockPostRepository.Setup(x => x.DeleteAsync(postId))
            .ReturnsAsync(true);

        _mockStorageService.Setup(x => x.DeleteFileAsync(It.IsAny<string>()))
            .ReturnsAsync(new StorageResult { Success = true });

        // Act
        var result = await _postService.DeletePostAsync(postId);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task DeletePostAsync_WithInvalidId_ReturnsError()
    {
        // Arrange
        var postId = Guid.NewGuid();

        _mockPostRepository.Setup(x => x.GetByIdAsync(postId))
            .ReturnsAsync((Post?)null);

        // Act
        var result = await _postService.DeletePostAsync(postId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }
} 