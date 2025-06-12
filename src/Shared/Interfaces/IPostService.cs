using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shared.Models;

namespace Shared.Interfaces;

public interface IPostService
{
    Task<IEnumerable<PostDto>> GetAllPostsAsync();
    Task<PostDto?> GetPostByIdAsync(Guid id);
    Task<PagedResult<PostDto>> GetPagedPostsAsync(int page, int pageSize);
    Task<PostDto> CreatePostAsync(CreatePostDto dto);
    Task<PostDto> UpdatePostAsync(Guid id, UpdatePostDto dto);
    Task DeletePostAsync(Guid id);
}

public class PostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PublicId { get; set; }
    public string? MediaUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? JsonMeta { get; set; }
}

public class CreatePostDto
{
    public string Title { get; set; } = string.Empty;
    public IFormFile? File { get; set; }
    public string? JsonMeta { get; set; }
}

public class UpdatePostDto
{
    public string? Title { get; set; }
    public IFormFile? File { get; set; }
    public string? JsonMeta { get; set; }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
} 