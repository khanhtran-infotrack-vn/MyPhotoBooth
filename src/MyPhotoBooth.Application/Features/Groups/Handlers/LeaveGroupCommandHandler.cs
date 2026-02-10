using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Common.Configuration;
using MyPhotoBooth.Application.Features.Groups.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class LeaveGroupCommandHandler : IRequestHandler<LeaveGroupCommand, Result>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IOptions<GroupSettings> _groupSettings;
    private readonly ILogger<LeaveGroupCommandHandler> _logger;

    public LeaveGroupCommandHandler(
        IGroupRepository groupRepository,
        IOptions<GroupSettings> groupSettings,
        ILogger<LeaveGroupCommandHandler> logger)
    {
        _groupRepository = groupRepository;
        _groupSettings = groupSettings;
        _logger = logger;
    }

    public async Task<Result> Handle(
        LeaveGroupCommand request,
        CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group == null)
            return Result.Failure(Errors.Groups.NotFound);

        if (group.IsDeleted)
            return Result.Failure(Errors.Groups.GroupIsDeleted);

        var member = await _groupRepository.GetMemberAsync(request.GroupId, request.UserId, cancellationToken);
        if (member == null || !member.IsActive)
            return Result.Failure(Errors.Groups.NotAMember);

        // If owner is leaving, schedule deletion
        if (group.OwnerId == request.UserId)
        {
            // Check if there are other active members
            var members = await _groupRepository.GetMembersAsync(request.GroupId, cancellationToken);
            var otherActiveMembers = members.Where(m => m.IsActive && m.UserId != request.UserId).ToList();

            if (otherActiveMembers.Any())
            {
                // Owner leaving with members - schedule deletion
                group.DeletionScheduledAt = DateTime.UtcNow;
                group.DeletionProcessDate = DateTime.UtcNow.AddDays(_groupSettings.Value.DeletionDays);
                await _groupRepository.UpdateAsync(group, cancellationToken);

                // TODO: Send deletion scheduled email to all members
                _logger.LogWarning("Owner left group {GroupId}, deletion scheduled for {DeletionDate}", group.Id, group.DeletionProcessDate);
            }
            else
            {
                // Owner is last member - delete immediately
                await _groupRepository.DeleteAsync(group.Id, cancellationToken);
                _logger.LogInformation("Last owner left group {GroupId}, deleted immediately", group.Id);
                return Result.Success();
            }
        }

        // Member leaving (not owner)
        member.LeftAt = DateTime.UtcNow;
        member.ContentRemovalDate = DateTime.UtcNow.AddDays(_groupSettings.Value.MemberContentGraceDays);
        await _groupRepository.UpdateMemberAsync(member, cancellationToken);

        // TODO: Send member left email

        _logger.LogInformation("Member left group: {GroupId} - {UserId}", group.Id, request.UserId);

        return Result.Success();
    }
}
