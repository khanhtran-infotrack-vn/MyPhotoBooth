using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Features.Albums.Commands;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Albums.Handlers;

public class RemovePhotosFromAlbumCommandHandler : IRequestHandler<RemovePhotosFromAlbumCommand, Result>
{
    private readonly IAlbumRepository _albumRepository;
    private readonly ILogger<RemovePhotosFromAlbumCommandHandler> _logger;

    public RemovePhotosFromAlbumCommandHandler(
        IAlbumRepository albumRepository,
        ILogger<RemovePhotosFromAlbumCommandHandler> logger)
    {
        _albumRepository = albumRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        RemovePhotosFromAlbumCommand request,
        CancellationToken cancellationToken)
    {
        var albumResult = await ValidateAlbumOwnershipAsync(request.UserId, request.AlbumId, cancellationToken);
        if (albumResult.IsFailure)
            return Result.Failure(albumResult.Error);

        foreach (var photoId in request.PhotoIds)
        {
            await _albumRepository.RemovePhotoFromAlbumAsync(request.AlbumId, photoId, cancellationToken);
            _logger.LogInformation("Photo {PhotoId} removed from album {AlbumId}", photoId, request.AlbumId);
        }

        return Result.Success();
    }

    private async Task<Result<Domain.Entities.Album>> ValidateAlbumOwnershipAsync(string userId, Guid albumId, CancellationToken cancellationToken)
    {
        var album = await _albumRepository.GetByIdAsync(albumId, cancellationToken);
        if (album == null)
            return Result.Failure<Domain.Entities.Album>(Errors.Albums.NotFound);
        if (album.UserId != userId)
            return Result.Failure<Domain.Entities.Album>(Errors.General.Unauthorized);
        return Result.Success(album);
    }
}
