using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Groups.Commands;

public record UpdateGroupCommand(
    Guid Id,
    string Name,
    string? Description,
    string UserId
) : ICommand<GroupResponse>;
