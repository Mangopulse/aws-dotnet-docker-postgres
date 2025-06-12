using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Storage;
using Xunit;

namespace Unit.Tests.Storage
{
    public class StorageProviderFactoryTests
    {
        [Theory]
        [InlineData("aws", typeof(AwsS3StorageProvider))]
        [InlineData("azure", typeof(AzureBlobStorageProvider))]
        [InlineData("local", typeof(LocalFileStorageProvider))]
        [InlineData(null, typeof(LocalFileStorageProvider))] // Default to local
        public void CreateProvider_ShouldReturnCorrectType(string providerType, Type expectedType)
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("StorageProvider", providerType)
                })
                .Build();

            // Act
            var provider = StorageProviderFactory.CreateProvider(configuration);

            // Assert
            Assert.IsType(expectedType, provider);
        }

        [Fact]
        public void CreateProvider_WithInvalidProvider_ShouldThrowArgumentException()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("StorageProvider", "invalid")
                })
                .Build();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => StorageProviderFactory.CreateProvider(configuration));
        }

        [Fact]
        public void AddStorageProvider_ShouldRegisterProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("StorageProvider", "local")
                })
                .Build();

            // Act
            services.AddStorageProvider(configuration);
            var provider = services.BuildServiceProvider().GetService<IStorageProvider>();

            // Assert
            Assert.NotNull(provider);
            Assert.IsType<LocalFileStorageProvider>(provider);
        }
    }
} 