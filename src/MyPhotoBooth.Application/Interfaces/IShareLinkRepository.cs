using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Interfaces;

public interface IShareLinkRepository
{
    Task<ShareLink?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<ShareLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ShareLink>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ShareLink>> GetActiveByPhotoIdAsync(Guid photoId, string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ShareLink>> GetActiveByAlbumIdAsync(Guid albumId, string userId, CancellationToken cancellationToken = default);
    Task<ShareLink> AddAsync(ShareLink shareLink, CancellationToken cancellationToken = default);
    Task UpdateAsync(ShareLink shareLink, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
