using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Models;

namespace API.Repositories;

public class PostRepository : IPostRepository
{
    private readonly ApplicationDbContext _context;

    public PostRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await _context.Posts
            .Include(p => p.Media)
            .ToListAsync();
    }

    public async Task<Post?> GetByIdAsync(Guid id)
    {
        return await _context.Posts
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Post> CreateAsync(Post post)
    {
        post.Id = Guid.NewGuid();
        post.CreatedAt = DateTime.UtcNow;
        
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        
        return post;
    }

    public async Task<Post> UpdateAsync(Post post)
    {
        post.UpdatedAt = DateTime.UtcNow;
        
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
        
        return post;
    }

    public async Task DeleteAsync(Guid id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post != null)
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }
    }
} 