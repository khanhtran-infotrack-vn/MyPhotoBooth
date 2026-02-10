using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Tags.Commands;

public record AddTagsToPhotoCommand(
    Guid PhotoId,
    List<Guid> TagIds,
    string UserId
) : ICommand;
