using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Groups.Queries;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class GetGroupQueryHandler : IRequestHandler<GetGroupQuery, Result<GroupDetailsResponse>>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<GetGroupQueryHandler> _logger;

    public GetGroupQueryHandler(
        IGroupRepository groupRepository,
        ILogger<GetGroupQueryHandler> logger)
    {
        _groupRepository = groupRepository;
        _logger = logger;
    }

    public async Task<Result<GroupDetailsResponse>> Handle(
        GetGroupQuery request,
        CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdWithMembersAsync(request.Id, cancellationToken);
        if (group == null)
            return Result.Failure<GroupDetailsResponse>(Errors.Groups.NotFound);

        if (group.IsDeleted)
            return Result.Failure<GroupDetailsResponse>(Errors.Groups.GroupIsDeleted);

        // Check if user is owner or member
        var isOwner = group.OwnerId == request.UserId;
        var isMember = await _groupRepository.IsUserMemberAsync(request.Id, request.UserId, cancellationToken);

        if (!isOwner && !isMember)
            return Result.Failure<GroupDetailsResponse>(Errors.Groups.NotAMember);

        // Get members
        var members = await _groupRepository.GetMembersAsync(request.Id, cancellationToken);
        var memberResponses = members.Select(m => new GroupMemberResponse
        {
            Id = m.Id,
            UserId = m.UserId,
            Email = m.User?.Email,
            JoinedAt = m.JoinedAt,
            IsActive = m.IsActive,
            IsInGracePeriod = m.IsInGracePeriod
        }).ToList();

        // Get shared content
        var sharedContent = await _groupRepository.GetSharedContentAsync(request.Id, cancellationToken);
        var contentResponses = sharedContent.Select(sc => new GroupContentResponse
        {
            Id = sc.Id,
            ContentType = sc.ContentType,
            PhotoId = sc.PhotoId,
            AlbumId = sc.AlbumId,
            PhotoName = null, // Will be populated if we include Photo/Album navigation
            AlbumName = null,
            SharedByUserId = sc.SharedByUserId,
            SharedAt = sc.SharedAt,
            IsActive = sc.IsActive
        }).ToList();

        var response = new GroupDetailsResponse
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            OwnerId = group.OwnerId,
            IsOwner = isOwner,
            IsDeleted = group.IsDeleted,
            IsDeletionScheduled = group.IsDeletionScheduled,
            DaysUntilDeletion = group.DaysUntilDeletion,
            DeletionProcessDate = group.DeletionProcessDate,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt,
            Members = memberResponses,
            SharedContent = contentResponses
        };

        _logger.LogInformation("Retrieved group details: {GroupId} for user {UserId}", group.Id, request.UserId);

        return Result.Success(response);
    }
}
