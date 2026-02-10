using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Auth.Commands;

public record LoginCommand(
    string Email,
    string Password
) : ICommand<AuthResponse>;
