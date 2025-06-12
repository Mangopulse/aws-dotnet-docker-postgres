using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Models;

namespace Shared.Interfaces;

public interface IMediaRepository
{
    Task<IEnumerable<Media>> GetAllAsync();
    Task<Media?> GetByIdAsync(Guid id);
    Task<Media> CreateAsync(Media media);
    Task<Media> UpdateAsync(Media media);
    Task DeleteAsync(Guid id);
} 