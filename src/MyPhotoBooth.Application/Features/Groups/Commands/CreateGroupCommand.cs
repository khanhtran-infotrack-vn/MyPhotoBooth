using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Groups.Commands;

public record CreateGroupCommand(
    string Name,
    string? Description,
    string UserId
) : ICommand<GroupResponse>;
