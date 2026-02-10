using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Albums.Queries;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Albums.Handlers;

public class GetAlbumQueryHandler : IRequestHandler<GetAlbumQuery, Result<AlbumDetailsResponse>>
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly ILogger<GetAlbumQueryHandler> _logger;

    public GetAlbumQueryHandler(
        IAlbumRepository albumRepository,
        IPhotoRepository photoRepository,
        ILogger<GetAlbumQueryHandler> logger)
    {
        _albumRepository = albumRepository;
        _photoRepository = photoRepository;
        _logger = logger;
    }

    public async Task<Result<AlbumDetailsResponse>> Handle(
        GetAlbumQuery request,
        CancellationToken cancellationToken)
    {
        var albumResult = await ValidateAlbumOwnershipAsync(request.UserId, request.AlbumId, cancellationToken);
        if (albumResult.IsFailure)
            return Result.Failure<AlbumDetailsResponse>(albumResult.Error);

        var album = albumResult.Value;

        // Get photo IDs for favorite status lookup
        var photoIds = album.AlbumPhotos.Select(ap => ap.Photo.Id).ToList();
        var favoriteStatus = photoIds.Any()
            ? await _photoRepository.GetFavoriteStatusAsync(photoIds, request.UserId, cancellationToken)
            : new Dictionary<Guid, bool>();

        return Result.Success(new AlbumDetailsResponse
        {
            Id = album.Id,
            Name = album.Name,
            Description = album.Description,
            CoverPhotoId = album.CoverPhotoId,
            CreatedAt = album.CreatedAt,
            UpdatedAt = album.UpdatedAt,
            Photos = album.AlbumPhotos.OrderBy(ap => ap.SortOrder).Select(ap => new PhotoListResponse
            {
                Id = ap.Photo.Id,
                OriginalFileName = ap.Photo.OriginalFileName,
                Width = ap.Photo.Width,
                Height = ap.Photo.Height,
                CapturedAt = ap.Photo.CapturedAt,
                UploadedAt = ap.Photo.UploadedAt,
                ThumbnailPath = ap.Photo.ThumbnailPath,
                IsFavorite = favoriteStatus.GetValueOrDefault(ap.Photo.Id, false)
            }).ToList()
        });
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
