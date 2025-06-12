using Shared.Interfaces;
using Shared.Models;
using Shared.Models.Dtos;
using Shared.Repositories;

namespace Shared.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;

    public PostService(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<IEnumerable<PostDto>> GetAllPostsAsync()
    {
        var posts = await _postRepository.GetAllAsync();
        return posts.Select(p => new PostDto
        {
            Id = p.Id,
            Title = p.Title,
            MediaId = p.MediaId,
            JsonMeta = p.JsonMeta,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task<PostDto?> GetPostByIdAsync(Guid id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null) return null;

        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            MediaId = post.MediaId,
            JsonMeta = post.JsonMeta,
            CreatedAt = post.CreatedAt
        };
    }

    public async Task<PostDto?> GetPostByPublicIdAsync(int publicId)
    {
        var post = await _postRepository.GetByPublicIdAsync(publicId);
        if (post == null) return null;

        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            MediaId = post.MediaId,
            JsonMeta = post.JsonMeta,
            CreatedAt = post.CreatedAt
        };
    }

    public async Task<PostDto> CreatePostAsync(CreatePostDto createPostDto)
    {
        var post = new Post
        {
            Title = createPostDto.Title,
            MediaId = createPostDto.MediaId ?? null,
            JsonMeta = createPostDto.JsonMeta,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _postRepository.CreateAsync(post);
        var createdPost = await GetPostByIdAsync(id);
        if (createdPost == null)
            throw new InvalidOperationException("Failed to create post");
            
        return createdPost;
    }

    public async Task<PostDto> UpdatePostAsync(Guid id, UpdatePostDto updatePostDto)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
            throw new KeyNotFoundException($"Post with ID {id} not found");

        post.Title = updatePostDto.Title;
        post.MediaId = updatePostDto.MediaId;
        post.JsonMeta = updatePostDto.JsonMeta;

        await _postRepository.UpdateAsync(post);
        var updatedPost = await GetPostByIdAsync(id);
        if (updatedPost == null)
            throw new InvalidOperationException("Failed to update post");
            
        return updatedPost;
    }

    public async Task DeletePostAsync(Guid id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
            throw new KeyNotFoundException($"Post with ID {id} not found");

        await _postRepository.DeleteAsync(id);
    }
} 