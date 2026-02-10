using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Features.Albums.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Albums.Handlers;

public class AddPhotosToAlbumCommandHandler : IRequestHandler<AddPhotosToAlbumCommand, Result>
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly ILogger<AddPhotosToAlbumCommandHandler> _logger;

    public AddPhotosToAlbumCommandHandler(
        IAlbumRepository albumRepository,
        IPhotoRepository photoRepository,
        ILogger<AddPhotosToAlbumCommandHandler> logger)
    {
        _albumRepository = albumRepository;
        _photoRepository = photoRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        AddPhotosToAlbumCommand request,
        CancellationToken cancellationToken)
    {
        var albumResult = await ValidateAlbumOwnershipAsync(request.UserId, request.AlbumId, cancellationToken);
        if (albumResult.IsFailure)
            return Result.Failure(albumResult.Error);

        var album = albumResult.Value;

        foreach (var photoId in request.PhotoIds)
        {
            var photoResult = await ValidatePhotoOwnershipAsync(request.UserId, photoId, cancellationToken);
            if (photoResult.IsFailure)
                return Result.Failure(photoResult.Error);

            var existingPhoto = album.AlbumPhotos.FirstOrDefault(ap => ap.PhotoId == photoId);
            if (existingPhoto != null)
                continue;

            var sortOrder = album.AlbumPhotos.Count;
            await _albumRepository.AddPhotoToAlbumAsync(request.AlbumId, photoId, sortOrder, cancellationToken);

            _logger.LogInformation("Photo {PhotoId} added to album {AlbumId}", photoId, request.AlbumId);
        }

        return Result.Success();
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

    private async Task<Result<Photo>> ValidatePhotoOwnershipAsync(string userId, Guid photoId, CancellationToken cancellationToken)
    {
        var photo = await _photoRepository.GetByIdAsync(photoId, cancellationToken);
        if (photo == null)
            return Result.Failure<Photo>(Errors.Photos.NotFound);
        if (photo.UserId != userId)
            return Result.Failure<Photo>(Errors.General.Unauthorized);
        return Result.Success(photo);
    }
}
