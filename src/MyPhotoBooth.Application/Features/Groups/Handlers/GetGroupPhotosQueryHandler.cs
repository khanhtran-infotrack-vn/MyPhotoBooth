using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Groups.Queries;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class GetGroupPhotosQueryHandler : IRequestHandler<GetGroupPhotosQuery, Result<List<GroupContentResponse>>>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<GetGroupPhotosQueryHandler> _logger;

    public GetGroupPhotosQueryHandler(
        IGroupRepository groupRepository,
        ILogger<GetGroupPhotosQueryHandler> logger)
    {
        _groupRepository = groupRepository;
        _logger = logger;
    }

    public async Task<Result<List<GroupContentResponse>>> Handle(
        GetGroupPhotosQuery request,
        CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group == null)
            return Result.Failure<List<GroupContentResponse>>(Errors.Groups.NotFound);

        if (group.IsDeleted)
            return Result.Failure<List<GroupContentResponse>>(Errors.Groups.GroupIsDeleted);

        // Check if user is owner or member
        var isOwner = group.OwnerId == request.UserId;
        var isMember = await _groupRepository.IsUserMemberAsync(request.GroupId, request.UserId, cancellationToken);

        if (!isOwner && !isMember)
            return Result.Failure<List<GroupContentResponse>>(Errors.Groups.NotAMember);

        var sharedContent = await _groupRepository.GetSharedContentAsync(request.GroupId, cancellationToken);

        var photos = sharedContent
            .Where(sc => sc.ContentType == SharedContentType.Photo && sc.IsActive)
            .Select(sc => new GroupContentResponse
            {
                Id = sc.Id,
                ContentType = sc.ContentType,
                PhotoId = sc.PhotoId,
                AlbumId = sc.AlbumId,
                PhotoName = null, // In real implementation, fetch from Photo entity
                AlbumName = null,
                SharedByUserId = sc.SharedByUserId,
                SharedAt = sc.SharedAt,
                IsActive = sc.IsActive
            })
            .ToList();

        _logger.LogInformation("Retrieved {Count} photos for group {GroupId}", photos.Count, request.GroupId);

        return Result.Success(photos);
    }
}
