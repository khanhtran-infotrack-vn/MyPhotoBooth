using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Features.Groups.Commands;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Groups.Handlers;

public class DeleteGroupCommandHandler : IRequestHandler<DeleteGroupCommand, Result>
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<DeleteGroupCommandHandler> _logger;

    public DeleteGroupCommandHandler(
        IGroupRepository groupRepository,
        ILogger<DeleteGroupCommandHandler> logger)
    {
        _groupRepository = groupRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteGroupCommand request,
        CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(request.Id, cancellationToken);
        if (group == null)
            return Result.Failure(Errors.Groups.NotFound);

        if (group.OwnerId != request.UserId)
            return Result.Failure(Errors.Groups.NotOwner);

        if (group.IsDeleted)
            return Result.Failure(Errors.Groups.GroupIsDeleted);

        await _groupRepository.DeleteAsync(group.Id, cancellationToken);

        _logger.LogInformation("Group deleted: {GroupId} by user {UserId}", group.Id, request.UserId);

        return Result.Success();
    }
}
