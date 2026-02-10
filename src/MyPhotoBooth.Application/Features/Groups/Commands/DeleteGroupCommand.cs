using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Groups.Commands;

public record DeleteGroupCommand(
    Guid Id,
    string UserId
) : ICommand;
