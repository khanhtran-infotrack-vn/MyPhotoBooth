using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Albums.Queries;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Albums.Handlers;

public class GetAlbumsQueryHandler : IRequestHandler<GetAlbumsQuery, Result<List<AlbumResponse>>>
{
    private readonly IAlbumRepository _albumRepository;
    private readonly ILogger<GetAlbumsQueryHandler> _logger;

    public GetAlbumsQueryHandler(
        IAlbumRepository albumRepository,
        ILogger<GetAlbumsQueryHandler> logger)
    {
        _albumRepository = albumRepository;
        _logger = logger;
    }

    public async Task<Result<List<AlbumResponse>>> Handle(
        GetAlbumsQuery request,
        CancellationToken cancellationToken)
    {
        var albums = await _albumRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        var albumList = albums.Select(a => new AlbumResponse
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description,
            CoverPhotoId = a.CoverPhotoId,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            PhotoCount = a.AlbumPhotos.Count
        }).ToList();

        return Result.Success(albumList);
    }
}
