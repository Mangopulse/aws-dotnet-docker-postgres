using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Storage
{
    public static class StorageProviderFactory
    {
        public static IStorageProvider CreateProvider(IConfiguration configuration)
        {
            var providerType = configuration["StorageProvider"]?.ToLower() ?? "local";

            return providerType switch
            {
                "aws" => new AwsS3StorageProvider(configuration),
                "azure" => new AzureBlobStorageProvider(configuration),
                "local" => new LocalFileStorageProvider(configuration),
                _ => throw new ArgumentException($"Unsupported storage provider: {providerType}")
            };
        }

        public static IServiceCollection AddStorageProvider(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IStorageProvider>(sp => CreateProvider(configuration));
            return services;
        }
    }
} 