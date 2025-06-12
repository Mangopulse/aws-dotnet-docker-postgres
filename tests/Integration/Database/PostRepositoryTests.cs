using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Shared.Data;
using Shared.Interfaces;
using Shared.Models;
using Shared.Repositories;
using System.Text.Json;
using Xunit;

namespace Integration.Tests.Database;

public class PostRepositoryTests : IDisposable
{
    private class PostMeta
    {
        public string Description { get; set; } = string.Empty;
    }

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
        
        // Create tables
        var schema = File.ReadAllText("schema.sql");
        _connection.Execute(schema);
    }

    public void Dispose()
    {
        // Clean up tables
        _connection.Execute("DROP TABLE IF EXISTS posts CASCADE");
        _connection.Execute("DROP TABLE IF EXISTS media CASCADE");
        _connection.Dispose();
    }

    [Fact]
    public async Task CreateAndGetPost_ShouldSucceed()
    {
        // Arrange
        var media = new Media
        {
            Id = Guid.NewGuid(),
            AwsS3Path = "test/path/image.jpg"
        };

        var post = new Post
        {
            Title = "Test Post",
            MediaId = media.Id
        };

        post.SetJsonMeta(new PostMeta { Description = "Test Description" });

        // Act
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(
            "INSERT INTO media (id, aws_s3_path) VALUES (@Id, @AwsS3Path)",
            media);

        var createdId = await _repository.CreateAsync(post);
        
        // Debug: Verify post exists in database
        var dbPost = await connection.QueryFirstOrDefaultAsync<Post>(
            "SELECT * FROM posts WHERE id = @Id",
            new { Id = createdId });
        Assert.NotNull(dbPost); // Verify post exists
        Assert.Equal(createdId, dbPost.Id); // Verify ID matches

        var retrievedPost = await _repository.GetByIdAsync(createdId);

        // Assert
        Assert.NotNull(retrievedPost);
        Assert.Equal(createdId, retrievedPost.Id);
        Assert.Equal(post.Title, retrievedPost.Title);
        Assert.Equal(post.MediaId, retrievedPost.MediaId);
        
        // Compare deserialized JSON objects instead of raw strings
        var expectedMeta = JsonSerializer.Deserialize<PostMeta>(post.JsonMeta);
        var actualMeta = JsonSerializer.Deserialize<PostMeta>(retrievedPost.JsonMeta);
        Assert.Equal(expectedMeta?.Description, actualMeta?.Description);
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