using Microsoft.Extensions.DependencyInjection;
using DockerX.Domain.Interfaces;
using DockerX.Infrastructure.Data;
using DockerX.Infrastructure.Repositories;

namespace DockerX.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IMediaRepository, MediaRepository>();

            return services;
        }
    }
} 