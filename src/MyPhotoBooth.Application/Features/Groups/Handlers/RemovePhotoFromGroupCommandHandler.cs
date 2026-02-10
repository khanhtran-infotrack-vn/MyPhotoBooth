using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Features.Groups.Commands;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class RemovePhotoFromGroupCommandHandler : IRequestHandler<RemovePhotoFromGroupCommand, Result>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<RemovePhotoFromGroupCommandHandler> _logger;

    public RemovePhotoFromGroupCommandHandler(
        IGroupRepository groupRepository,
        ILogger<RemovePhotoFromGroupCommandHandler> logger)
    {
        _groupRepository = groupRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        RemovePhotoFromGroupCommand request,
        CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group == null)
            return Result.Failure(Errors.Groups.NotFound);

        if (group.IsDeleted)
            return Result.Failure(Errors.Groups.GroupIsDeleted);

        // Get shared content
        var sharedContent = await _groupRepository.GetSharedContentAsync(request.GroupId, cancellationToken);
        var photoContent = sharedContent.FirstOrDefault(sc =>
            sc.ContentType == SharedContentType.Photo &&
            sc.PhotoId == request.PhotoId &&
            sc.IsActive);

        if (photoContent == null)
            return Result.Failure(Errors.Groups.ContentNotShared);

        // Check permissions: owner can remove any, members can only remove their own
        if (group.OwnerId != request.UserId && photoContent.SharedByUserId != request.UserId)
            return Result.Failure(Errors.Groups.UnauthorizedAccess);

        // Soft remove
        photoContent.RemovedAt = DateTime.UtcNow;
        await _groupRepository.UpdateSharedContentAsync(photoContent, cancellationToken);

        _logger.LogInformation("Photo removed from group: {PhotoId} <- {GroupId}", request.PhotoId, request.GroupId);

        return Result.Success();
    }
}
