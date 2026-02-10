using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Auth.Commands;

public record LogoutCommand(
    string RefreshToken
) : ICommand;
