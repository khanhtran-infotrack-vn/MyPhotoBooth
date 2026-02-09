using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Interfaces;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tag?> GetByNameAsync(string name, string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> SearchAsync(string query, string userId, CancellationToken cancellationToken = default);
    Task<Tag> AddAsync(Tag tag, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
