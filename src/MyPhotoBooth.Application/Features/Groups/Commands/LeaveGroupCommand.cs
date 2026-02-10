using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Groups.Commands;

public record LeaveGroupCommand(
    Guid GroupId,
    string UserId
) : ICommand;
