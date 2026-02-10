using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Auth.Commands;

public record RegisterCommand(
    string Email,
    string Password,
    string DisplayName
) : ICommand;
