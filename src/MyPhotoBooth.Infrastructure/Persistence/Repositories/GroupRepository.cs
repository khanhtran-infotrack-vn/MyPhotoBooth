using Microsoft.EntityFrameworkCore;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using MyPhotoBooth.Infrastructure.Persistence;

namespace MyPhotoBooth.Infrastructure.Persistence.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly AppDbContext _context;

    public GroupRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<Group?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<Group?> GetByIdWithContentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .Include(g => g.SharedContent)
                .ThenInclude(sc => sc.Photo)
            .Include(g => g.SharedContent)
                .ThenInclude(sc => sc.Album)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Group>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .Where(g => g.OwnerId == ownerId && !g.DeletedAt.HasValue)
            .OrderByDescending(g => g.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Group>> GetByMemberIdAsync(string memberId, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .Include(g => g.Members)
            .Where(g => g.Members.Any(m => m.UserId == memberId && !m.LeftAt.HasValue) && !g.DeletedAt.HasValue)
            .OrderByDescending(g => g.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<GroupMember?> GetMemberAsync(Guid groupId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId && !gm.LeftAt.HasValue, cancellationToken);
    }

    public async Task<IEnumerable<GroupMember>> GetMembersAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupMembers
            .Include(gm => gm.User)
            .Where(gm => gm.GroupId == groupId && !gm.LeftAt.HasValue)
            .OrderBy(gm => gm.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<GroupSharedContent>> GetSharedContentAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupSharedContents
            .Include(gsc => gsc.Photo)
            .Include(gsc => gsc.Album)
            .Where(gsc => gsc.GroupId == groupId && !gsc.RemovedAt.HasValue)
            .OrderByDescending(gsc => gsc.SharedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsUserMemberAsync(Guid groupId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && !gm.LeftAt.HasValue, cancellationToken);
    }

    public async Task<bool> IsUserOwnerAsync(Guid groupId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .AnyAsync(g => g.Id == groupId && g.OwnerId == userId && !g.DeletedAt.HasValue, cancellationToken);
    }

    public async Task<int> GetMemberCountAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupMembers
            .CountAsync(gm => gm.GroupId == groupId && !gm.LeftAt.HasValue, cancellationToken);
    }

    public async Task<Group> AddAsync(Group group, CancellationToken cancellationToken = default)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);
        return group;
    }

    public async Task<GroupMember> AddMemberAsync(GroupMember member, CancellationToken cancellationToken = default)
    {
        _context.GroupMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);
        return member;
    }

    public async Task<GroupSharedContent> AddSharedContentAsync(GroupSharedContent content, CancellationToken cancellationToken = default)
    {
        _context.GroupSharedContents.Add(content);
        await _context.SaveChangesAsync(cancellationToken);
        return content;
    }

    public async Task UpdateAsync(Group group, CancellationToken cancellationToken = default)
    {
        _context.Groups.Update(group);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateMemberAsync(GroupMember member, CancellationToken cancellationToken = default)
    {
        _context.GroupMembers.Update(member);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateSharedContentAsync(GroupSharedContent content, CancellationToken cancellationToken = default)
    {
        _context.GroupSharedContents.Update(content);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var group = await _context.Groups.FindAsync(new object[] { id }, cancellationToken);
        if (group != null)
        {
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
