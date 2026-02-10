using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Albums.Commands;

public record UpdateAlbumCommand(
    Guid AlbumId,
    string Name,
    string? Description,
    Guid? CoverPhotoId,
    string UserId
) : ICommand;
