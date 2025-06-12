using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Models;

namespace API.Repositories;

public class MediaRepository : IMediaRepository
{
    private readonly ApplicationDbContext _context;

    public MediaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Media>> GetAllAsync()
    {
        return await _context.Media.ToListAsync();
    }

    public async Task<Media?> GetByIdAsync(Guid id)
    {
        return await _context.Media.FindAsync(id);
    }

    public async Task<Media> CreateAsync(Media media)
    {
        media.Id = Guid.NewGuid();
        media.CreatedAt = DateTime.UtcNow;
        
        _context.Media.Add(media);
        await _context.SaveChangesAsync();
        
        return media;
    }

    public async Task<Media> UpdateAsync(Media media)
    {
        media.UpdatedAt = DateTime.UtcNow;
        
        _context.Media.Update(media);
        await _context.SaveChangesAsync();
        
        return media;
    }

    public async Task DeleteAsync(Guid id)
    {
        var media = await _context.Media.FindAsync(id);
        if (media != null)
        {
            _context.Media.Remove(media);
            await _context.SaveChangesAsync();
        }
    }
} 