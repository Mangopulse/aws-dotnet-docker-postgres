using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Shared.Storage;

namespace Integration.Tests.Storage
{
    public abstract class StorageProviderTests : IDisposable
    {
        protected abstract IStorageProvider CreateProvider();
        protected abstract string GetTestContainer();

        [Fact]
        public async Task UploadAndDownload_ShouldSucceed()
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
                var uploadResult = await provider.UploadAsync(container, fileName, stream);
                var downloadStream = await provider.DownloadAsync(container, fileName);
                var downloadedContent = await new StreamReader(downloadStream).ReadToEndAsync();

                // Assert
                Assert.NotNull(uploadResult);
                Assert.Equal(fileName, uploadResult.FileName);
                Assert.Equal(content, downloadedContent);
            }
            finally
            {
                // Cleanup
                await provider.DeleteAsync(container, fileName);
            }
        }

        [Fact]
        public async Task Delete_ShouldSucceed()
        {
            // Arrange
            var provider = CreateProvider();
            var container = GetTestContainer();
            var fileName = $"test-{Guid.NewGuid()}.txt";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test content"));

            // Act
            await provider.UploadAsync(container, fileName, stream);
            await provider.DeleteAsync(container, fileName);

            // Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => 
                provider.DownloadAsync(container, fileName));
        }

        [Fact]
        public async Task GetUrl_ShouldReturnValidUrl()
        {
            // Arrange
            var provider = CreateProvider();
            var container = GetTestContainer();
            var fileName = $"test-{Guid.NewGuid()}.txt";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test content"));

            try
            {
                // Act
                await provider.UploadAsync(container, fileName, stream);
                var url = await provider.GetUrlAsync(container, fileName);

                // Assert
                Assert.NotNull(url);
                Assert.True(Uri.TryCreate(url, UriKind.Absolute, out _));
            }
            finally
            {
                await provider.DeleteAsync(container, fileName);
            }
        }

        public virtual void Dispose()
        {
            // Base implementation does nothing
        }
    }
} 