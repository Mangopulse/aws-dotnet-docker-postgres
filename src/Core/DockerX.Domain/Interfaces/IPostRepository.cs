using DockerX.Domain.Entities;

namespace DockerX.Domain.Interfaces;

public interface IPostRepository
{
    Task<IEnumerable<Post>> GetAllAsync();
    Task<Post?> GetByIdAsync(Guid id);
    Task<Post> CreateAsync(Post post);
    Task<Post> UpdateAsync(Post post);
    Task DeleteAsync(Guid id);
} 