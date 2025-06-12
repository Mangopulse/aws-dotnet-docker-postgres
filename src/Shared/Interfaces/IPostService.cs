using Shared.Models;
using Shared.Models.Dtos;

namespace Shared.Interfaces;

public interface IPostService
{
    Task<IEnumerable<PostDto>> GetAllPostsAsync();
    Task<PostDto?> GetPostByIdAsync(Guid id);
    Task<PostDto?> GetPostByPublicIdAsync(int publicId);
    Task<PostDto> CreatePostAsync(CreatePostDto createPostDto);
    Task<PostDto> UpdatePostAsync(Guid id, UpdatePostDto updatePostDto);
    Task DeletePostAsync(Guid id);
} 