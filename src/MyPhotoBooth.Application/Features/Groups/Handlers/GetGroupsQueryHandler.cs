using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Groups.Queries;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class GetGroupsQueryHandler : IRequestHandler<GetGroupsQuery, Result<List<GroupResponse>>>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<GetGroupsQueryHandler> _logger;

    public GetGroupsQueryHandler(
        IGroupRepository groupRepository,
        ILogger<GetGroupsQueryHandler> logger)
    {
        _groupRepository = groupRepository;
        _logger = logger;
    }

    public async Task<Result<List<GroupResponse>>> Handle(
        GetGroupsQuery request,
        CancellationToken cancellationToken)
    {
        // Get groups owned by user
        var ownedGroups = await _groupRepository.GetByOwnerIdAsync(request.UserId, cancellationToken);

        // Get groups where user is a member
        var memberGroups = await _groupRepository.GetByMemberIdAsync(request.UserId, cancellationToken);

        // Combine and deduplicate
        var allGroups = ownedGroups
            .UnionBy(memberGroups, g => g.Id)
            .Where(g => !g.IsDeleted)
            .OrderByDescending(g => g.UpdatedAt)
            .ToList();

        var responses = new List<GroupResponse>();

        foreach (var group in allGroups)
        {
            var memberCount = await _groupRepository.GetMemberCountAsync(group.Id, cancellationToken);
            var isOwner = group.OwnerId == request.UserId;

            responses.Add(new GroupResponse
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                OwnerId = group.OwnerId,
                IsOwner = isOwner,
                MemberCount = memberCount,
                ContentCount = 0, // Will be populated if needed
                IsDeleted = group.IsDeleted,
                IsDeletionScheduled = group.IsDeletionScheduled,
                DaysUntilDeletion = group.DaysUntilDeletion,
                CreatedAt = group.CreatedAt,
                UpdatedAt = group.UpdatedAt
            });
        }

        _logger.LogInformation("Retrieved {Count} groups for user {UserId}", responses.Count, request.UserId);

        return Result.Success(responses);
    }
}
