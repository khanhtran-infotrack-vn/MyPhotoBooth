using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Commands;

public record DeletePhotoCommand(
    Guid PhotoId,
    string UserId
) : ICommand;
