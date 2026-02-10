using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Auth.Commands;

public record ForgotPasswordCommand(
    string Email,
    string ResetUrlTemplate
) : ICommand;
