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
}
