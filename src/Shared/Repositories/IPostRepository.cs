using Shared.Models;

namespace Shared.Repositories;

public interface IPostRepository
{
    Task<IEnumerable<Post>> GetAllAsync();
    Task<Post?> GetByIdAsync(Guid id);
    Task<Post?> GetByPublicIdAsync(int publicId);
    Task<Guid> CreateAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Guid id);
} 