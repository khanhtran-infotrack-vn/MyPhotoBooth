using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Tags.Commands;

public record RemoveTagFromPhotoCommand(
    Guid PhotoId,
    Guid TagId,
    string UserId
) : ICommand;
