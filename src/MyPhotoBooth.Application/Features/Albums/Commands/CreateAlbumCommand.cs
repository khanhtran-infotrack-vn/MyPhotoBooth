using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Albums.Commands;

public record CreateAlbumCommand(
    string Name,
    string? Description,
    string UserId
) : ICommand<AlbumResponse>;
