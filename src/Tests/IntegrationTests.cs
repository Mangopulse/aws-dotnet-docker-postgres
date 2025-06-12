using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shared.Models;
using Xunit;

namespace Tests
{
    public class IntegrationTests : IAsyncLifetime
    {
        private ApplicationDbContext _context = null!;
        private PostRepository _postRepository = null!;
        private MediaRepository _mediaRepository = null!;
        private Guid _mediaId;
        private Guid _postId;

        public async Task InitializeAsync()
        {
            // Load test configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Testing.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            // Set up PostgreSQL connection using Docker container
            var connectionString = config.GetConnectionString("DefaultConnection");
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            _context = new ApplicationDbContext(options);

            // Ensure database exists and apply migrations if needed
            try
            {
                // Check if we can connect to the database
                await _context.Database.CanConnectAsync();
                
                // Apply any pending migrations
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await _context.Database.MigrateAsync();
                }
            }
            catch
            {
                // If connection fails or migrations fail, ensure database is created
                await _context.Database.EnsureCreatedAsync();
            }

            // Clean any existing test data
            await CleanTestData();

            // Create a Media object
            var media = new Media
            {
                Id = Guid.NewGuid(),
                AwsS3Path = "test/path/image.jpg",
                CreatedAt = DateTime.UtcNow
            };
            _mediaId = media.Id;
            _context.Media.Add(media);
            await _context.SaveChangesAsync();

            // Create a Post referencing the Media
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Title = "Integration Test Post",
                MediaId = media.Id,
                PublicId = 1,
                JsonMeta = "{}",
                CreatedAt = DateTime.UtcNow
            };
            _postId = post.Id;
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            _postRepository = new PostRepository(_context);
            _mediaRepository = new MediaRepository(_context);
        }

        private async Task CleanTestData()
        {
            // Remove any existing test data
            var existingPosts = await _context.Posts.ToListAsync();
            _context.Posts.RemoveRange(existingPosts);
            
            var existingMedia = await _context.Media.ToListAsync();
            _context.Media.RemoveRange(existingMedia);
            
            await _context.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            // Clean up: delete test data
            await CleanTestData();

            // Dispose context
            await _context.DisposeAsync();
        }

        [Fact]
        public async Task CreateAndRetrievePost_WithMedia_Succeeds()
        {
            // Act
            var posts = await _postRepository.GetAllAsync();
            var post = posts.FirstOrDefault(p => p.Id == _postId);
            var media = await _mediaRepository.GetByIdAsync(_mediaId);

            // Assert
            Assert.NotNull(post);
            Assert.Equal(_mediaId, post.MediaId);
            Assert.Equal("Integration Test Post", post.Title);
            Assert.NotNull(media);
            Assert.Equal("test/path/image.jpg", media.AwsS3Path);
        }

        [Fact]
        public async Task CreatePost_ShouldGenerateUniqueId()
        {
            // Arrange
            var newPost = new Post
            {
                Title = "New Test Post",
                MediaId = _mediaId,
                JsonMeta = "{\"test\": true}"
            };

            // Act
            var createdPost = await _postRepository.CreateAsync(newPost);

            // Assert
            Assert.NotEqual(Guid.Empty, createdPost.Id);
            Assert.Equal("New Test Post", createdPost.Title);
            Assert.True(createdPost.CreatedAt > DateTime.MinValue);
            Assert.Equal(_mediaId, createdPost.MediaId);
        }

        [Fact]
        public async Task Database_ShouldBeAccessible()
        {
            // Act & Assert
            var canConnect = await _context.Database.CanConnectAsync();
            Assert.True(canConnect, "Should be able to connect to the test database");

            // Verify tables exist
            var postTableExists = await _context.Database.SqlQueryRaw<int>(
                "SELECT 1 FROM information_schema.tables WHERE table_name = 'Posts'").AnyAsync();
            var mediaTableExists = await _context.Database.SqlQueryRaw<int>(
                "SELECT 1 FROM information_schema.tables WHERE table_name = 'Media'").AnyAsync();

            Assert.True(postTableExists, "Posts table should exist");
            Assert.True(mediaTableExists, "Media table should exist");
        }
    }
} 