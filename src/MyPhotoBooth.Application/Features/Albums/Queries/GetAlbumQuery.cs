using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Albums.Queries;

public record GetAlbumQuery(
    Guid AlbumId,
    string UserId
) : IQuery<AlbumDetailsResponse>;
