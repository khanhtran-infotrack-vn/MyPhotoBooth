using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Features.Groups.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class RemoveGroupMemberCommandHandler : IRequestHandler<RemoveGroupMemberCommand, Result>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<RemoveGroupMemberCommandHandler> _logger;

    public RemoveGroupMemberCommandHandler(
        IGroupRepository groupRepository,
        ILogger<RemoveGroupMemberCommandHandler> logger)
    {
        _groupRepository = groupRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        RemoveGroupMemberCommand request,
        CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group == null)
            return Result.Failure(Errors.Groups.NotFound);

        if (group.OwnerId != request.UserId)
            return Result.Failure(Errors.Groups.NotOwner);

        if (group.IsDeleted)
            return Result.Failure(Errors.Groups.GroupIsDeleted);

        var member = await _groupRepository.GetMemberAsync(request.GroupId, request.MemberUserId, cancellationToken);
        if (member == null || !member.IsActive)
            return Result.Failure(Errors.Groups.MemberNotFound);

        // Cannot remove the owner
        if (member.UserId == group.OwnerId)
            return Result.Failure(Errors.Groups.CannotRemoveOwner);

        // Soft remove - set LeftAt
        member.LeftAt = DateTime.UtcNow;
        member.ContentRemovalDate = DateTime.UtcNow.AddDays(7); // TODO: Use GroupSettings
        await _groupRepository.UpdateMemberAsync(member, cancellationToken);

        // TODO: Send member removed email

        _logger.LogInformation("Member removed from group: {GroupId} - {MemberId}", group.Id, member.Id);

        return Result.Success();
    }
}
