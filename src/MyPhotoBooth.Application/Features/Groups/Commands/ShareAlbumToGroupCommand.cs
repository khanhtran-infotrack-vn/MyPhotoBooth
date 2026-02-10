using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Groups.Commands;

public record ShareAlbumToGroupCommand(
    Guid GroupId,
    Guid AlbumId,
    string UserId
) : ICommand;
