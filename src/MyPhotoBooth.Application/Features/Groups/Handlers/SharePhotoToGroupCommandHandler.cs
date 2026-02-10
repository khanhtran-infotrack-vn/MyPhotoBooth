using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Features.Groups.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class SharePhotoToGroupCommandHandler : IRequestHandler<SharePhotoToGroupCommand, Result>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly ILogger<SharePhotoToGroupCommandHandler> _logger;

    public SharePhotoToGroupCommandHandler(
        IGroupRepository groupRepository,
        IPhotoRepository photoRepository,
        ILogger<SharePhotoToGroupCommandHandler> logger)
    {
        _groupRepository = groupRepository;
        _photoRepository = photoRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        SharePhotoToGroupCommand request,
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

        // Verify photo exists and belongs to user
        var photo = await _photoRepository.GetByIdAsync(request.PhotoId, cancellationToken);
        if (photo == null)
            return Result.Failure(Errors.Groups.PhotoNotFound);

        if (photo.UserId != request.UserId)
            return Result.Failure(Errors.Photos.UnauthorizedAccess);

        // Check if already shared
        var sharedContent = await _groupRepository.GetSharedContentAsync(request.GroupId, cancellationToken);
        var alreadyShared = sharedContent.Any(sc =>
            sc.ContentType == SharedContentType.Photo &&
            sc.PhotoId == request.PhotoId &&
            sc.IsActive);

        if (alreadyShared)
            return Result.Success(); // Already shared, return success

        // Share photo to group
        var groupContent = new GroupSharedContent
        {
            Id = Guid.NewGuid(),
            GroupId = request.GroupId,
            SharedByUserId = request.UserId,
            ContentType = SharedContentType.Photo,
            PhotoId = request.PhotoId,
            SharedAt = DateTime.UtcNow
        };

        await _groupRepository.AddSharedContentAsync(groupContent, cancellationToken);

        _logger.LogInformation("Photo shared to group: {PhotoId} -> {GroupId}", request.PhotoId, request.GroupId);

        return Result.Success();
    }
}
