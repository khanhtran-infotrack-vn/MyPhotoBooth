using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Interfaces;

public interface IAlbumRepository
{
    Task<Album?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Album>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Album> AddAsync(Album album, CancellationToken cancellationToken = default);
    Task UpdateAsync(Album album, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddPhotoToAlbumAsync(Guid albumId, Guid photoId, int sortOrder, CancellationToken cancellationToken = default);
    Task RemovePhotoFromAlbumAsync(Guid albumId, Guid photoId, CancellationToken cancellationToken = default);
}
