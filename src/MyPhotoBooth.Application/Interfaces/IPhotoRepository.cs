using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Interfaces;

public interface IPhotoRepository
{
    Task<Photo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Photo>> GetByUserIdAsync(string userId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task<int> GetCountByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Photo>> GetTimelineAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task<int> GetTimelineCountAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<Photo> AddAsync(Photo photo, CancellationToken cancellationToken = default);
    Task UpdateAsync(Photo photo, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsFavoriteAsync(Guid photoId, string userId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, bool>> GetFavoriteStatusAsync(IEnumerable<Guid> photoIds, string userId, CancellationToken cancellationToken = default);
    Task ToggleFavoriteAsync(Guid photoId, string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Photo>> GetFavoritesAsync(string userId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task<int> GetFavoritesCountAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Photo>> SearchAsync(string userId, string searchTerm, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task<int> GetSearchCountAsync(string userId, string searchTerm, CancellationToken cancellationToken = default);

    // Bulk operations
    Task<List<Photo>> GetByIdsAsync(List<Guid> ids, string userId, CancellationToken cancellationToken = default);
    Task<List<Photo>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
    Task DeleteMultipleAsync(List<Guid> ids, string userId, CancellationToken cancellationToken = default);
    Task AddToFavoritesAsync(List<Guid> photoIds, string userId, CancellationToken cancellationToken = default);
    Task RemoveFromFavoritesAsync(List<Guid> photoIds, string userId, CancellationToken cancellationToken = default);
}
