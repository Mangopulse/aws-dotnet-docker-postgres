using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Shared.Storage;
using Xunit;

namespace Integration.Tests.Storage
{
    public class AzureBlobStorageProviderTests : StorageProviderTests
    {
        private readonly IConfiguration _configuration;

        public AzureBlobStorageProviderTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .AddEnvironmentVariables()
                .Build();
        }

        protected override IStorageProvider CreateProvider()
        {
            return new AzureBlobStorageProvider(_configuration);
        }

        protected override string GetTestContainer()
        {
            return $"test-container-{Guid.NewGuid()}";
        }

        [Fact]
        public async Task UploadWithMetadata_ShouldSucceed()
        {
            // Arrange
            var provider = CreateProvider();
            var container = GetTestContainer();
            var fileName = $"metadata-{Guid.NewGuid()}.txt";
            var content = "Test content with metadata";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            try
            {
                // Act
                var result = await provider.UploadAsync(container, fileName, stream);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(fileName, result.FileName);
                Assert.Equal("text/plain", result.ContentType);
            }
            finally
            {
                await provider.DeleteAsync(container, fileName);
            }
        }
    }
} 