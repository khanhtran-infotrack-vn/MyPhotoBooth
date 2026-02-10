using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Groups.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, Result<GroupResponse>>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<CreateGroupCommandHandler> _logger;

    public CreateGroupCommandHandler(
        IGroupRepository groupRepository,
        ILogger<CreateGroupCommandHandler> logger)
    {
        _groupRepository = groupRepository;
        _logger = logger;
    }

    public async Task<Result<GroupResponse>> Handle(
        CreateGroupCommand request,
        CancellationToken cancellationToken)
    {
        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            OwnerId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add owner as first member
        var ownerMember = new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = request.UserId,
            JoinedAt = DateTime.UtcNow
        };

        await _groupRepository.AddAsync(group, cancellationToken);
        await _groupRepository.AddMemberAsync(ownerMember, cancellationToken);

        _logger.LogInformation("Group created: {GroupId} for user {UserId}", group.Id, request.UserId);

        return Result.Success(new GroupResponse
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            OwnerId = group.OwnerId,
            IsOwner = true,
            MemberCount = 1,
            ContentCount = 0,
            IsDeleted = false,
            IsDeletionScheduled = false,
            DaysUntilDeletion = 0,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt
        });
    }
}
