using Microsoft.EntityFrameworkCore;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Infrastructure.Persistence.Repositories;

public class TagRepository : ITagRepository
{
    private readonly AppDbContext _context;

    public TagRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tags.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Tag?> GetByNameAsync(string name, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Name == name && t.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Tag>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tag>> SearchAsync(string query, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .Where(t => t.UserId == userId && t.Name.Contains(query))
            .OrderBy(t => t.Name)
            .Take(10)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tag> AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync(cancellationToken);
        return tag;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await _context.Tags.FindAsync(new object[] { id }, cancellationToken);
        if (tag != null)
        {
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
