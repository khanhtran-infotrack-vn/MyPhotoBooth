using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Features.Groups.Commands;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class RemoveAlbumFromGroupCommandHandler : IRequestHandler<RemoveAlbumFromGroupCommand, Result>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<RemoveAlbumFromGroupCommandHandler> _logger;

    public RemoveAlbumFromGroupCommandHandler(
        IGroupRepository groupRepository,
        ILogger<RemoveAlbumFromGroupCommandHandler> logger)
    {
        _groupRepository = groupRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        RemoveAlbumFromGroupCommand request,
        CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group == null)
            return Result.Failure(Errors.Groups.NotFound);

        if (group.IsDeleted)
            return Result.Failure(Errors.Groups.GroupIsDeleted);

        // Get shared content
        var sharedContent = await _groupRepository.GetSharedContentAsync(request.GroupId, cancellationToken);
        var albumContent = sharedContent.FirstOrDefault(sc =>
            sc.ContentType == SharedContentType.Album &&
            sc.AlbumId == request.AlbumId &&
            sc.IsActive);

        if (albumContent == null)
            return Result.Failure(Errors.Groups.ContentNotShared);

        // Check permissions: owner can remove any, members can only remove their own
        if (group.OwnerId != request.UserId && albumContent.SharedByUserId != request.UserId)
            return Result.Failure(Errors.Groups.UnauthorizedAccess);

        // Soft remove
        albumContent.RemovedAt = DateTime.UtcNow;
        await _groupRepository.UpdateSharedContentAsync(albumContent, cancellationToken);

        _logger.LogInformation("Album removed from group: {AlbumId} <- {GroupId}", request.AlbumId, request.GroupId);

        return Result.Success();
    }
}
