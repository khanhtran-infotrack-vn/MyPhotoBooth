using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Albums.Queries;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Albums.Handlers;

public class GetAlbumPhotosQueryHandler : IRequestHandler<GetAlbumPhotosQuery, Result<List<PhotoListResponse>>>
{
    private readonly IAlbumRepository _albumRepository;
    private readonly ILogger<GetAlbumPhotosQueryHandler> _logger;

    public GetAlbumPhotosQueryHandler(
        IAlbumRepository albumRepository,
        ILogger<GetAlbumPhotosQueryHandler> logger)
    {
        _albumRepository = albumRepository;
        _logger = logger;
    }

    public async Task<Result<List<PhotoListResponse>>> Handle(
        GetAlbumPhotosQuery request,
        CancellationToken cancellationToken)
    {
        var albumResult = await ValidateAlbumOwnershipAsync(request.UserId, request.AlbumId, cancellationToken);
        if (albumResult.IsFailure)
            return Result.Failure<List<PhotoListResponse>>(albumResult.Error);

        var album = albumResult.Value;

        var photos = album.AlbumPhotos
            .OrderBy(ap => ap.SortOrder)
            .Select(ap => new PhotoListResponse
            {
                Id = ap.Photo.Id,
                OriginalFileName = ap.Photo.OriginalFileName,
                Width = ap.Photo.Width,
                Height = ap.Photo.Height,
                CapturedAt = ap.Photo.CapturedAt,
                UploadedAt = ap.Photo.UploadedAt,
                ThumbnailPath = ap.Photo.ThumbnailPath
            }).ToList();

        return Result.Success(photos);
    }

    private async Task<Result<Album>> ValidateAlbumOwnershipAsync(string userId, Guid albumId, CancellationToken cancellationToken)
    {
        var album = await _albumRepository.GetByIdAsync(albumId, cancellationToken);
        if (album == null)
            return Result.Failure<Album>(Errors.Albums.NotFound);
        if (album.UserId != userId)
            return Result.Failure<Album>(Errors.General.Unauthorized);
        return Result.Success(album);
    }
}
