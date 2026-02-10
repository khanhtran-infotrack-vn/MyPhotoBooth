using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Groups.Commands;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class UpdateGroupCommandHandler : IRequestHandler<UpdateGroupCommand, Result<GroupResponse>>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<UpdateGroupCommandHandler> _logger;

    public UpdateGroupCommandHandler(
        IGroupRepository groupRepository,
        ILogger<UpdateGroupCommandHandler> logger)
    {
        _groupRepository = groupRepository;
        _logger = logger;
    }

    public async Task<Result<GroupResponse>> Handle(
        UpdateGroupCommand request,
        CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(request.Id, cancellationToken);
        if (group == null)
            return Result.Failure<GroupResponse>(Errors.Groups.NotFound);

        if (group.OwnerId != request.UserId)
            return Result.Failure<GroupResponse>(Errors.Groups.NotOwner);

        if (group.IsDeleted)
            return Result.Failure<GroupResponse>(Errors.Groups.GroupIsDeleted);

        group.Name = request.Name;
        group.Description = request.Description;
        group.UpdatedAt = DateTime.UtcNow;

        await _groupRepository.UpdateAsync(group, cancellationToken);

        _logger.LogInformation("Group updated: {GroupId} by user {UserId}", group.Id, request.UserId);

        var memberCount = await _groupRepository.GetMemberCountAsync(group.Id, cancellationToken);

        return Result.Success(new GroupResponse
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            OwnerId = group.OwnerId,
            IsOwner = true,
            MemberCount = memberCount,
            ContentCount = 0, // Will be populated if needed
            IsDeleted = group.IsDeleted,
            IsDeletionScheduled = group.IsDeletionScheduled,
            DaysUntilDeletion = group.DaysUntilDeletion,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt
        });
    }
}
