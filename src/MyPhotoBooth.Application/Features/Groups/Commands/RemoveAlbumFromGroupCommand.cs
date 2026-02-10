using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Groups.Commands;

public record RemoveAlbumFromGroupCommand(
    Guid GroupId,
    Guid AlbumId,
    string UserId
) : ICommand;
