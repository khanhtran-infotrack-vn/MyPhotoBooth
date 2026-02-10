using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Groups.Queries;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class GetGroupAlbumsQueryHandler : IRequestHandler<GetGroupAlbumsQuery, Result<List<GroupContentResponse>>>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<GetGroupAlbumsQueryHandler> _logger;

    public GetGroupAlbumsQueryHandler(
        IGroupRepository groupRepository,
        ILogger<GetGroupAlbumsQueryHandler> logger)
    {
        _groupRepository = groupRepository;
        _logger = logger;
    }

    public async Task<Result<List<GroupContentResponse>>> Handle(
        GetGroupAlbumsQuery request,
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

        var albums = sharedContent
            .Where(sc => sc.ContentType == SharedContentType.Album && sc.IsActive)
            .Select(sc => new GroupContentResponse
            {
                Id = sc.Id,
                ContentType = sc.ContentType,
                PhotoId = sc.PhotoId,
                AlbumId = sc.AlbumId,
                PhotoName = null,
                AlbumName = null, // In real implementation, fetch from Album entity
                SharedByUserId = sc.SharedByUserId,
                SharedAt = sc.SharedAt,
                IsActive = sc.IsActive
            })
            .ToList();

        _logger.LogInformation("Retrieved {Count} albums for group {GroupId}", albums.Count, request.GroupId);

        return Result.Success(albums);
    }
}
