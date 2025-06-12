using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Shared.Storage;
using Xunit;

namespace Integration.Tests.Storage
{
    public class LocalFileStorageProviderTests : StorageProviderTests
    {
        private readonly IConfiguration _configuration;
        private readonly string _testBasePath;

        public LocalFileStorageProviderTests()
        {
            _testBasePath = Path.Combine(Path.GetTempPath(), "LocalStorageTests", Guid.NewGuid().ToString());
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("LocalStorage:BasePath", _testBasePath)
                })
                .Build();
        }

        protected override IStorageProvider CreateProvider()
        {
            return new LocalFileStorageProvider(_configuration);
        }

        protected override string GetTestContainer()
        {
            return $"test-container-{Guid.NewGuid()}";
        }

        public override void Dispose()
        {
            if (Directory.Exists(_testBasePath))
            {
                Directory.Delete(_testBasePath, true);
            }
            base.Dispose();
        }

        [Fact]
        public async Task CreateContainer_ShouldSucceed()
        {
            // Arrange
            var provider = CreateProvider();
            var container = GetTestContainer();
            var fileName = $"test-{Guid.NewGuid()}.txt";
            var content = "Test content";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            try
            {
                // Act
                await provider.UploadAsync(container, fileName, stream);

                // Assert
                var containerPath = Path.Combine(_testBasePath, container);
                Assert.True(Directory.Exists(containerPath));
                Assert.True(File.Exists(Path.Combine(containerPath, fileName)));
            }
            finally
            {
                await provider.DeleteAsync(container, fileName);
            }
        }

        [Fact]
        public async Task GetUrl_ShouldReturnLocalPath()
        {
            // Arrange
            var provider = CreateProvider();
            var container = GetTestContainer();
            var fileName = $"test-{Guid.NewGuid()}.txt";
            var content = "Test content";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            try
            {
                // Act
                await provider.UploadAsync(container, fileName, stream);
                var url = await provider.GetUrlAsync(container, fileName);

                // Assert
                Assert.StartsWith("file://", url);
                Assert.EndsWith(fileName, url);
            }
            finally
            {
                await provider.DeleteAsync(container, fileName);
            }
        }
    }
} 