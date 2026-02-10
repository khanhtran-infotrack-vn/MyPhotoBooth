using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Groups.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class TransferOwnershipCommandHandler : IRequestHandler<TransferOwnershipCommand, Result<GroupMemberResponse>>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<TransferOwnershipCommandHandler> _logger;

    public TransferOwnershipCommandHandler(
        IGroupRepository groupRepository,
        ILogger<TransferOwnershipCommandHandler> logger)
    {
        _groupRepository = groupRepository;
        _logger = logger;
    }

    public async Task<Result<GroupMemberResponse>> Handle(
        TransferOwnershipCommand request,
        CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group == null)
            return Result.Failure<GroupMemberResponse>(Errors.Groups.NotFound);

        if (group.OwnerId != request.UserId)
            return Result.Failure<GroupMemberResponse>(Errors.Groups.NotOwner);

        if (group.IsDeleted)
            return Result.Failure<GroupMemberResponse>(Errors.Groups.GroupIsDeleted);

        if (request.UserId == request.NewOwnerId)
            return Result.Failure<GroupMemberResponse>(Errors.Groups.CannotTransferToSelf);

        // Check if new owner is a member
        var newOwnerMember = await _groupRepository.GetMemberAsync(request.GroupId, request.NewOwnerId, cancellationToken);
        if (newOwnerMember == null || !newOwnerMember.IsActive)
            return Result.Failure<GroupMemberResponse>(Errors.Groups.NotAMember);

        // Transfer ownership
        var oldOwnerId = group.OwnerId;
        group.OwnerId = request.NewOwnerId;
        group.UpdatedAt = DateTime.UtcNow;

        // Cancel deletion if scheduled
        if (group.IsDeletionScheduled)
        {
            group.DeletionScheduledAt = null;
            group.DeletionProcessDate = null;
        }

        await _groupRepository.UpdateAsync(group, cancellationToken);

        // TODO: Send ownership transferred email

        _logger.LogInformation("Ownership transferred for group {GroupId} from {OldOwnerId} to {NewOwnerId}",
            group.Id, oldOwnerId, request.NewOwnerId);

        return Result.Success(new GroupMemberResponse
        {
            Id = newOwnerMember.Id,
            UserId = newOwnerMember.UserId,
            Email = request.NewOwnerId, // In real implementation, fetch from UserManager
            JoinedAt = newOwnerMember.JoinedAt,
            IsActive = true,
            IsInGracePeriod = false
        });
    }
}
