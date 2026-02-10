using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Commands;

public record UpdatePhotoCommand(
    Guid PhotoId,
    string? Description,
    string UserId
) : ICommand;
