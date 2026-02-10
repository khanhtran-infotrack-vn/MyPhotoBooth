using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Features.Groups.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class ShareAlbumToGroupCommandHandler : IRequestHandler<ShareAlbumToGroupCommand, Result>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IAlbumRepository _albumRepository;
    private readonly ILogger<ShareAlbumToGroupCommandHandler> _logger;

    public ShareAlbumToGroupCommandHandler(
        IGroupRepository groupRepository,
        IAlbumRepository albumRepository,
        ILogger<ShareAlbumToGroupCommandHandler> logger)
    {
        _groupRepository = groupRepository;
        _albumRepository = albumRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ShareAlbumToGroupCommand request,
        CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group == null)
            return Result.Failure(Errors.Groups.NotFound);

        if (group.IsDeleted)
            return Result.Failure(Errors.Groups.GroupIsDeleted);

        // Check if user is a member
        var isMember = await _groupRepository.IsUserMemberAsync(request.GroupId, request.UserId, cancellationToken);
        if (!isMember)
            return Result.Failure(Errors.Groups.NotAMember);

        // Verify album exists and belongs to user
        var album = await _albumRepository.GetByIdAsync(request.AlbumId, cancellationToken);
        if (album == null)
            return Result.Failure(Errors.Groups.AlbumNotFound);

        if (album.UserId != request.UserId)
            return Result.Failure(Errors.Albums.UnauthorizedAccess);

        // Check if already shared
        var sharedContent = await _groupRepository.GetSharedContentAsync(request.GroupId, cancellationToken);
        var alreadyShared = sharedContent.Any(sc =>
            sc.ContentType == SharedContentType.Album &&
            sc.AlbumId == request.AlbumId &&
            sc.IsActive);

        if (alreadyShared)
            return Result.Success(); // Already shared, return success

        // Share album to group
        var groupContent = new GroupSharedContent
        {
            Id = Guid.NewGuid(),
            GroupId = request.GroupId,
            SharedByUserId = request.UserId,
            ContentType = SharedContentType.Album,
            AlbumId = request.AlbumId,
            SharedAt = DateTime.UtcNow
        };

        await _groupRepository.AddSharedContentAsync(groupContent, cancellationToken);

        _logger.LogInformation("Album shared to group: {AlbumId} -> {GroupId}", request.AlbumId, request.GroupId);

        return Result.Success();
    }
}
