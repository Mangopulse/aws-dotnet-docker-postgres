using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Interfaces;
using Shared.Models;

namespace Shared.Services;

public class PostService : IPostService
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;
    private readonly ILogger<PostService> _logger;

    public PostService(
        ApplicationDbContext context,
        IStorageService storageService,
        ILogger<PostService> logger)
    {
        _context = context;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<IEnumerable<PostDto>> GetAllPostsAsync()
    {
        var posts = await _context.Posts
            .Include(p => p.Media)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return posts.Select(MapToDto);
    }

    public async Task<PostDto?> GetPostByIdAsync(Guid id)
    {
        var post = await _context.Posts
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == id);

        return post != null ? MapToDto(post) : null;
    }

    public async Task<PagedResult<PostDto>> GetPagedPostsAsync(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _context.Posts
            .Include(p => p.Media)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<PostDto>
        {
            Items = posts.Select(MapToDto),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<PostDto> CreatePostAsync(CreatePostDto dto)
    {
        Guid? mediaId = null;

        if (dto.File != null)
        {
            var filePath = await _storageService.UploadFileAsync(dto.File);
            var media = new Media
            {
                Id = Guid.NewGuid(),
                AwsS3Path = filePath,
                CreatedAt = DateTime.UtcNow
            };

            _context.Media.Add(media);
            await _context.SaveChangesAsync();
            mediaId = media.Id;
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            MediaId = mediaId ?? Guid.Empty,
            JsonMeta = dto.JsonMeta ?? "{}",
            CreatedAt = DateTime.UtcNow
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        return await GetPostByIdAsync(post.Id) ?? throw new InvalidOperationException("Failed to create post");
    }

    public async Task<PostDto> UpdatePostAsync(Guid id, UpdatePostDto dto)
    {
        var post = await _context.Posts
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            throw new KeyNotFoundException($"Post with ID {id} not found");
        }

        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            post.Title = dto.Title;
        }

        if (!string.IsNullOrWhiteSpace(dto.JsonMeta))
        {
            post.JsonMeta = dto.JsonMeta;
        }

        if (dto.File != null)
        {
            var filePath = await _storageService.UploadFileAsync(dto.File);
            var media = new Media
            {
                Id = Guid.NewGuid(),
                AwsS3Path = filePath,
                CreatedAt = DateTime.UtcNow
            };

            _context.Media.Add(media);
            await _context.SaveChangesAsync();

            // Delete old media if exists
            if (post.Media != null)
            {
                await _storageService.DeleteFileAsync(post.Media.AwsS3Path);
                _context.Media.Remove(post.Media);
            }

            post.MediaId = media.Id;
        }

        post.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetPostByIdAsync(post.Id) ?? throw new InvalidOperationException("Failed to update post");
    }

    public async Task DeletePostAsync(Guid id)
    {
        var post = await _context.Posts
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            throw new KeyNotFoundException($"Post with ID {id} not found");
        }

        if (post.Media != null)
        {
            await _storageService.DeleteFileAsync(post.Media.AwsS3Path);
            _context.Media.Remove(post.Media);
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
    }

    private static PostDto MapToDto(Post post)
    {
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            PublicId = post.PublicId,
            MediaUrl = post.Media?.AwsS3Path,
            CreatedAt = post.CreatedAt,
            JsonMeta = post.JsonMeta
        };
    }
} 