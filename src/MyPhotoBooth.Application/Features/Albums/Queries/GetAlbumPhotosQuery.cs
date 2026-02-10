using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Albums.Queries;

public record GetAlbumPhotosQuery(
    Guid AlbumId,
    string UserId
) : IQuery<List<PhotoListResponse>>;
