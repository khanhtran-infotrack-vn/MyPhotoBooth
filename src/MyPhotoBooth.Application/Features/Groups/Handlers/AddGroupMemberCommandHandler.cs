using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Common.Configuration;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Groups.Commands;
using MyPhotoBooth.Application.Features.Groups.Queries;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class AddGroupMemberCommandHandler : IRequestHandler<AddGroupMemberCommand, Result<GroupMemberResponse>>
{
    private readonly IGroupRepository _groupRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IOptions<GroupSettings> _groupSettings;
    private readonly ILogger<AddGroupMemberCommandHandler> _logger;

    public AddGroupMemberCommandHandler(
        IGroupRepository groupRepository,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IOptions<GroupSettings> groupSettings,
        ILogger<AddGroupMemberCommandHandler> logger)
    {
        _groupRepository = groupRepository;
        _userManager = userManager;
        _emailService = emailService;
        _groupSettings = groupSettings;
        _logger = logger;
    }

    public async Task<Result<GroupMemberResponse>> Handle(
        AddGroupMemberCommand request,
        CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group == null)
            return Result.Failure<GroupMemberResponse>(Errors.Groups.NotFound);

        if (group.OwnerId != request.UserId)
            return Result.Failure<GroupMemberResponse>(Errors.Groups.NotOwner);

        if (group.IsDeleted)
            return Result.Failure<GroupMemberResponse>(Errors.Groups.GroupIsDeleted);

        // Find user by email
        var memberUser = await _userManager.FindByEmailAsync(request.MemberEmail);
        if (memberUser == null)
            return Result.Failure<GroupMemberResponse>(Errors.Groups.UserNotFound);

        // Check if already a member
        var existingMember = await _groupRepository.GetMemberAsync(request.GroupId, memberUser.Id, cancellationToken);
        if (existingMember != null && existingMember.IsActive)
            return Result.Failure<GroupMemberResponse>(Errors.Groups.AlreadyAMember);

        // Check max members limit
        var memberCount = await _groupRepository.GetMemberCountAsync(request.GroupId, cancellationToken);
        if (memberCount >= _groupSettings.Value.MaxMembersPerGroup)
            return Result.Failure<GroupMemberResponse>(Errors.Groups.GroupFull);

        // Add member
        var newMember = new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = request.GroupId,
            UserId = memberUser.Id,
            JoinedAt = DateTime.UtcNow
        };

        await _groupRepository.AddMemberAsync(newMember, cancellationToken);

        // TODO: Send member added email
        // await _emailService.SendGroupMemberAddedEmailAsync(...);

        _logger.LogInformation("Member added to group: {GroupId} - {UserId}", group.Id, memberUser.Id);

        return Result.Success(new GroupMemberResponse
        {
            Id = newMember.Id,
            UserId = newMember.UserId,
            Email = request.MemberEmail,
            JoinedAt = newMember.JoinedAt,
            IsActive = true,
            IsInGracePeriod = false
        });
    }
}
