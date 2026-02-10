using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Albums.Commands;

public record RemovePhotosFromAlbumCommand(
    Guid AlbumId,
    List<Guid> PhotoIds,
    string UserId
) : ICommand;
