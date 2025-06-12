using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Shared.Data;
using Shared.Models;
using Shared.Repositories;
using Xunit;

namespace Shared.Tests.Repositories;

public class PostRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseContext _context;
    private readonly IPostRepository _repository;
    private readonly IDbConnection _connection;

    public PostRepositoryTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .Build();

        _context = new DatabaseContext(configuration);
        _repository = new PostRepository(_context);
        _connection = _context.CreateConnection();
    }

    public async Task InitializeAsync()
    {
        // Create tables
        var schema = await File.ReadAllTextAsync("../../../Shared/Data/schema.sql");
        await _connection.ExecuteAsync(schema);
    }

    public async Task DisposeAsync()
    {
        // Clean up tables
        await _connection.ExecuteAsync("DROP TABLE IF EXISTS posts CASCADE");
        await _connection.ExecuteAsync("DROP TABLE IF EXISTS media CASCADE");
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task CreateAndGetPost_ShouldSucceed()
    {
        // Arrange
        var media = new Media
        {
            Id = Guid.NewGuid(),
            AwsS3Path = "test/path/image.jpg",
            CreatedAt = DateTime.UtcNow
        };

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Test Post",
            MediaId = media.Id,
            JsonMeta = "{\"description\":\"Test Description\"}",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _connection.ExecuteAsync(
            "INSERT INTO media (id, aws_s3_path, created_at) VALUES (@Id, @AwsS3Path, @CreatedAt)",
            media);

        var createdId = await _repository.CreateAsync(post);
        var retrievedPost = await _repository.GetByIdAsync(createdId);

        // Assert
        Assert.NotNull(retrievedPost);
        Assert.Equal(post.Id, retrievedPost.Id);
        Assert.Equal(post.Title, retrievedPost.Title);
        Assert.Equal(post.MediaId, retrievedPost.MediaId);
        Assert.Equal(post.JsonMeta, retrievedPost.JsonMeta);
    }

    [Fact]
    public async Task GetAllPosts_ShouldReturnAllPosts()
    {
        // Arrange
        var media = new Media
        {
            Id = Guid.NewGuid(),
            AwsS3Path = "test/path/image.jpg",
            CreatedAt = DateTime.UtcNow
        };

        var post1 = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Test Post 1",
            MediaId = media.Id,
            JsonMeta = "{\"description\":\"Test Description 1\"}",
            CreatedAt = DateTime.UtcNow
        };

        var post2 = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Test Post 2",
            MediaId = media.Id,
            JsonMeta = "{\"description\":\"Test Description 2\"}",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _connection.ExecuteAsync(
            "INSERT INTO media (id, aws_s3_path, created_at) VALUES (@Id, @AwsS3Path, @CreatedAt)",
            media);

        await _repository.CreateAsync(post1);
        await _repository.CreateAsync(post2);

        var posts = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, posts.Count());
        Assert.Contains(posts, p => p.Id == post1.Id);
        Assert.Contains(posts, p => p.Id == post2.Id);
    }
} 