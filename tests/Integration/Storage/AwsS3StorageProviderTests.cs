using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Shared.Storage;
using Xunit;

namespace Integration.Tests.Storage
{
    public class AwsS3StorageProviderTests : StorageProviderTests
    {
        private readonly IConfiguration _configuration;

        public AwsS3StorageProviderTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .AddEnvironmentVariables()
                .Build();
        }

        protected override IStorageProvider CreateProvider()
        {
            return new AwsS3StorageProvider(_configuration);
        }

        protected override string GetTestContainer()
        {
            return $"test-container-{Guid.NewGuid()}";
        }

        [Fact]
        public async Task UploadLargeFile_ShouldSucceed()
        {
            // Arrange
            var provider = CreateProvider();
            var container = GetTestContainer();
            var fileName = $"large-{Guid.NewGuid()}.txt";
            var content = new byte[5 * 1024 * 1024]; // 5MB
            new Random().NextBytes(content);
            var stream = new MemoryStream(content);

            try
            {
                // Act
                var result = await provider.UploadAsync(container, fileName, stream);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(fileName, result.FileName);
                Assert.Equal(content.Length, result.Size);
            }
            finally
            {
                await provider.DeleteAsync(container, fileName);
            }
        }
    }
} 