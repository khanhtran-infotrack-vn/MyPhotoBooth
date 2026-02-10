using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Groups.Queries;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class GetGroupMembersQueryHandler : IRequestHandler<GetGroupMembersQuery, Result<List<GroupMemberResponse>>>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<GetGroupMembersQueryHandler> _logger;

    public GetGroupMembersQueryHandler(
        IGroupRepository groupRepository,
        ILogger<GetGroupMembersQueryHandler> logger)
    {
        _groupRepository = groupRepository;
        _logger = logger;
    }

    public async Task<Result<List<GroupMemberResponse>>> Handle(
        GetGroupMembersQuery request,
        CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group == null)
            return Result.Failure<List<GroupMemberResponse>>(Errors.Groups.NotFound);

        if (group.IsDeleted)
            return Result.Failure<List<GroupMemberResponse>>(Errors.Groups.GroupIsDeleted);

        // Check if user is owner or member
        var isOwner = group.OwnerId == request.UserId;
        var isMember = await _groupRepository.IsUserMemberAsync(request.GroupId, request.UserId, cancellationToken);

        if (!isOwner && !isMember)
            return Result.Failure<List<GroupMemberResponse>>(Errors.Groups.NotAMember);

        var members = await _groupRepository.GetMembersAsync(request.GroupId, cancellationToken);

        var responses = members.Select(m => new GroupMemberResponse
        {
            Id = m.Id,
            UserId = m.UserId,
            Email = m.User?.Email,
            JoinedAt = m.JoinedAt,
            IsActive = m.IsActive,
            IsInGracePeriod = m.IsInGracePeriod
        }).ToList();

        _logger.LogInformation("Retrieved {Count} members for group {GroupId}", responses.Count, request.GroupId);

        return Result.Success(responses);
    }
}
