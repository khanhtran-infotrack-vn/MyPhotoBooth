using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Albums.Commands;

public record DeleteAlbumCommand(
    Guid AlbumId,
    string UserId
) : ICommand;
