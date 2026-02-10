using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Albums.Queries;

public record GetAlbumsQuery(
    string UserId
) : IQuery<List<AlbumResponse>>;
