using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Interfaces;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Group?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Group?> GetByIdWithContentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Group>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Group>> GetByMemberIdAsync(string memberId, CancellationToken cancellationToken = default);
    Task<GroupMember?> GetMemberAsync(Guid groupId, string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GroupMember>> GetMembersAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GroupSharedContent>> GetSharedContentAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<bool> IsUserMemberAsync(Guid groupId, string userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserOwnerAsync(Guid groupId, string userId, CancellationToken cancellationToken = default);
    Task<int> GetMemberCountAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<Group> AddAsync(Group group, CancellationToken cancellationToken = default);
    Task<GroupMember> AddMemberAsync(GroupMember member, CancellationToken cancellationToken = default);
    Task<GroupSharedContent> AddSharedContentAsync(GroupSharedContent content, CancellationToken cancellationToken = default);
    Task UpdateAsync(Group group, CancellationToken cancellationToken = default);
    Task UpdateMemberAsync(GroupMember member, CancellationToken cancellationToken = default);
    Task UpdateSharedContentAsync(GroupSharedContent content, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
