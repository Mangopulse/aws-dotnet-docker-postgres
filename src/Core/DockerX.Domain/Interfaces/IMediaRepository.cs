using DockerX.Domain.Entities;

namespace DockerX.Domain.Interfaces;

public interface IMediaRepository
{
    Task<IEnumerable<Media>> GetAllAsync();
    Task<Media?> GetByIdAsync(Guid id);
    Task<Media> CreateAsync(Media media);
    Task<Media> UpdateAsync(Media media);
    Task DeleteAsync(Guid id);
} 