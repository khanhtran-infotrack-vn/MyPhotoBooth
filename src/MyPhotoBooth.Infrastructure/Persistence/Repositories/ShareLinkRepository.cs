using Microsoft.EntityFrameworkCore;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Infrastructure.Persistence.Repositories;

public class ShareLinkRepository : IShareLinkRepository
{
    private readonly AppDbContext _context;

    public ShareLinkRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ShareLink?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.ShareLinks
            .Include(sl => sl.Photo)
            .Include(sl => sl.Album)
                .ThenInclude(a => a!.AlbumPhotos)
                .ThenInclude(ap => ap.Photo)
            .FirstOrDefaultAsync(sl => sl.Token == token, cancellationToken);
    }

    public async Task<ShareLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ShareLinks
            .Include(sl => sl.Photo)
            .Include(sl => sl.Album)
            .FirstOrDefaultAsync(sl => sl.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ShareLink>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.ShareLinks
            .Include(sl => sl.Photo)
            .Include(sl => sl.Album)
            .Where(sl => sl.UserId == userId)
            .OrderByDescending(sl => sl.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ShareLink>> GetActiveByPhotoIdAsync(Guid photoId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.ShareLinks
            .Where(sl => sl.PhotoId == photoId && sl.UserId == userId && sl.RevokedAt == null)
            .OrderByDescending(sl => sl.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ShareLink>> GetActiveByAlbumIdAsync(Guid albumId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.ShareLinks
            .Where(sl => sl.AlbumId == albumId && sl.UserId == userId && sl.RevokedAt == null)
            .OrderByDescending(sl => sl.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ShareLink> AddAsync(ShareLink shareLink, CancellationToken cancellationToken = default)
    {
        _context.ShareLinks.Add(shareLink);
        await _context.SaveChangesAsync(cancellationToken);
        return shareLink;
    }

    public async Task UpdateAsync(ShareLink shareLink, CancellationToken cancellationToken = default)
    {
        _context.ShareLinks.Update(shareLink);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var shareLink = await _context.ShareLinks.FindAsync(new object[] { id }, cancellationToken);
        if (shareLink != null)
        {
            _context.ShareLinks.Remove(shareLink);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
