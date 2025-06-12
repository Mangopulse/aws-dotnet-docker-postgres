using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Models;

namespace Shared.Interfaces;

public interface IPostRepository
{
    Task<IEnumerable<Post>> GetAllAsync();
    Task<Post?> GetByIdAsync(Guid id);
    Task<Post> CreateAsync(Post post);
    Task<Post> UpdateAsync(Post post);
    Task DeleteAsync(Guid id);
} 