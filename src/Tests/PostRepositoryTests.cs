using System;
using System.Threading.Tasks;
using API.Data;
using API.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using Xunit;

namespace Tests
{
    public class PostRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PostRepository _repository;

        public PostRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new PostRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetPost_ShouldReturnPost_WhenPostExists()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var post = new Post { Id = postId, Title = "Test Post" };
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(postId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(postId, result.Id);
            Assert.Equal("Test Post", result.Title);
        }
    }
} 