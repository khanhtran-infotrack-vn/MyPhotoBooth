using Microsoft.EntityFrameworkCore;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Infrastructure.Persistence.Repositories;

public class PhotoRepository : IPhotoRepository
{
    private readonly AppDbContext _context;

    public PhotoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Photo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Photos
            .Include(p => p.PhotoTags)
            .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Photo>> GetByUserIdAsync(string userId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        return await _context.Photos
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.UploadedAt)
            .Skip(skip)
            .Take(take)
            .Include(p => p.PhotoTags)
            .ThenInclude(pt => pt.Tag)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Photos
            .Where(p => p.UserId == userId)
            .CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Photo>> GetTimelineAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        var query = _context.Photos.Where(p => p.UserId == userId);

        if (fromDate.HasValue)
            query = query.Where(p => p.CapturedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.CapturedAt <= toDate.Value);

        return await query
            .OrderByDescending(p => p.CapturedAt ?? p.UploadedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTimelineCountAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Photos.Where(p => p.UserId == userId);

        if (fromDate.HasValue)
            query = query.Where(p => p.CapturedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.CapturedAt <= toDate.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Photo> AddAsync(Photo photo, CancellationToken cancellationToken = default)
    {
        _context.Photos.Add(photo);
        await _context.SaveChangesAsync(cancellationToken);
        return photo;
    }

    public async Task UpdateAsync(Photo photo, CancellationToken cancellationToken = default)
    {
        _context.Photos.Update(photo);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var photo = await _context.Photos.FindAsync(new object[] { id }, cancellationToken);
        if (photo != null)
        {
            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> IsFavoriteAsync(Guid photoId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.FavoritePhotos
            .AnyAsync(fp => fp.PhotoId == photoId && fp.UserId == userId, cancellationToken);
    }

    public async Task ToggleFavoriteAsync(Guid photoId, string userId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.FavoritePhotos
            .FirstOrDefaultAsync(fp => fp.UserId == userId && fp.PhotoId == photoId, cancellationToken);

        if (existing != null)
        {
            _context.FavoritePhotos.Remove(existing);
        }
        else
        {
            var favorite = new FavoritePhoto
            {
                Id = Guid.NewGuid(),
                PhotoId = photoId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.FavoritePhotos.Add(favorite);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Photo>> GetFavoritesAsync(string userId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        return await _context.FavoritePhotos
            .Where(fp => fp.UserId == userId)
            .OrderByDescending(fp => fp.CreatedAt)
            .Select(fp => fp.Photo)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetFavoritesCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.FavoritePhotos
            .CountAsync(fp => fp.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Photo>> SearchAsync(string userId, string searchTerm, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLowerInvariant();

        // Get photos by tag names
        var photoIdsByTags = await _context.PhotoTags
            .Where(pt => pt.Tag.Name.ToLower().Contains(term))
            .Where(pt => pt.Photo.UserId == userId)
            .Select(pt => pt.PhotoId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Get photos by album names
        var photoIdsByAlbums = await _context.AlbumPhotos
            .Where(ap => ap.Album.Name.ToLower().Contains(term))
            .Where(ap => ap.Album.UserId == userId)
            .Select(ap => ap.PhotoId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var allMatchingPhotoIds = photoIdsByTags
            .Concat(photoIdsByAlbums)
            .ToHashSet();

        // Main query searching by filename, description, and matching tag/album IDs
        return await _context.Photos
            .Where(p => p.UserId == userId)
            .Where(p =>
                allMatchingPhotoIds.Contains(p.Id) ||
                p.OriginalFileName.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)))
            .OrderByDescending(p => p.UploadedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetSearchCountAsync(string userId, string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLowerInvariant();

        // Get photos by tag names
        var photoIdsByTags = await _context.PhotoTags
            .Where(pt => pt.Tag.Name.ToLower().Contains(term))
            .Where(pt => pt.Photo.UserId == userId)
            .Select(pt => pt.PhotoId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Get photos by album names
        var photoIdsByAlbums = await _context.AlbumPhotos
            .Where(ap => ap.Album.Name.ToLower().Contains(term))
            .Where(ap => ap.Album.UserId == userId)
            .Select(ap => ap.PhotoId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var allMatchingPhotoIds = photoIdsByTags
            .Concat(photoIdsByAlbums)
            .ToHashSet();

        return await _context.Photos
            .Where(p => p.UserId == userId)
            .Where(p =>
                allMatchingPhotoIds.Contains(p.Id) ||
                p.OriginalFileName.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)))
            .CountAsync(cancellationToken);
    }
}
