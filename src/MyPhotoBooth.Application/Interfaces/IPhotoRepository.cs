using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Interfaces;

public interface IPhotoRepository
{
    Task<Photo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Photo>> GetByUserIdAsync(string userId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task<IEnumerable<Photo>> GetTimelineAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task<Photo> AddAsync(Photo photo, CancellationToken cancellationToken = default);
    Task UpdateAsync(Photo photo, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
