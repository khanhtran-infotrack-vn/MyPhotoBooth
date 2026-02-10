using Microsoft.EntityFrameworkCore;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Infrastructure.Persistence.Repositories;

public class AlbumRepository : IAlbumRepository
{
    private readonly AppDbContext _context;

    public AlbumRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Album?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Albums
            .Include(a => a.AlbumPhotos)
            .ThenInclude(ap => ap.Photo)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Album>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Albums
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Album> AddAsync(Album album, CancellationToken cancellationToken = default)
    {
        _context.Albums.Add(album);
        await _context.SaveChangesAsync(cancellationToken);
        return album;
    }

    public async Task UpdateAsync(Album album, CancellationToken cancellationToken = default)
    {
        _context.Albums.Update(album);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var album = await _context.Albums.FindAsync(new object[] { id }, cancellationToken);
        if (album != null)
        {
            _context.Albums.Remove(album);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddPhotoToAlbumAsync(Guid albumId, Guid photoId, int sortOrder, CancellationToken cancellationToken = default)
    {
        var albumPhoto = new AlbumPhoto
        {
            AlbumId = albumId,
            PhotoId = photoId,
            AddedAt = DateTime.UtcNow,
            SortOrder = sortOrder
        };
        
        _context.AlbumPhotos.Add(albumPhoto);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePhotoFromAlbumAsync(Guid albumId, Guid photoId, CancellationToken cancellationToken = default)
    {
        var albumPhoto = await _context.AlbumPhotos
            .FirstOrDefaultAsync(ap => ap.AlbumId == albumId && ap.PhotoId == photoId, cancellationToken);

        if (albumPhoto != null)
        {
            _context.AlbumPhotos.Remove(albumPhoto);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // Bulk operations

    public async Task<bool> UserOwnsAlbumAsync(Guid albumId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Albums
            .AnyAsync(a => a.Id == albumId && a.UserId == userId, cancellationToken);
    }

    public async Task<List<AlbumPhoto>> GetAlbumPhotosAsync(Guid albumId, List<Guid> photoIds, CancellationToken cancellationToken = default)
    {
        return await _context.AlbumPhotos
            .Where(ap => ap.AlbumId == albumId && photoIds.Contains(ap.PhotoId))
            .ToListAsync(cancellationToken);
    }

    public async Task AddPhotosToAlbumAsync(Guid albumId, List<Guid> photoIds, string userId, CancellationToken cancellationToken = default)
    {
        // Verify album ownership
        var album = await _context.Albums
            .FirstOrDefaultAsync(a => a.Id == albumId && a.UserId == userId, cancellationToken);

        if (album == null)
        {
            throw new UnauthorizedAccessException("User does not own this album");
        }

        // Get existing album photos
        var existingPhotoIds = await _context.AlbumPhotos
            .Where(ap => ap.AlbumId == albumId)
            .Select(ap => ap.PhotoId)
            .ToListAsync(cancellationToken);

        // Get max sort order
        var maxSortOrder = existingPhotoIds.Count > 0
            ? await _context.AlbumPhotos
                .Where(ap => ap.AlbumId == albumId)
                .Select(ap => (int?)ap.SortOrder)
                .MaxAsync(cancellationToken) ?? 0
            : 0;

        // Filter out photos already in album
        var newPhotoIds = photoIds.Except(existingPhotoIds).ToList();

        var albumPhotos = new List<AlbumPhoto>();
        foreach (var photoId in newPhotoIds)
        {
            maxSortOrder++;
            albumPhotos.Add(new AlbumPhoto
            {
                AlbumId = albumId,
                PhotoId = photoId,
                AddedAt = DateTime.UtcNow,
                SortOrder = maxSortOrder
            });
        }

        await _context.AlbumPhotos.AddRangeAsync(albumPhotos, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePhotosFromAlbumAsync(Guid albumId, List<Guid> photoIds, CancellationToken cancellationToken = default)
    {
        var albumPhotos = await _context.AlbumPhotos
            .Where(ap => ap.AlbumId == albumId && photoIds.Contains(ap.PhotoId))
            .ToListAsync(cancellationToken);

        _context.AlbumPhotos.RemoveRange(albumPhotos);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
