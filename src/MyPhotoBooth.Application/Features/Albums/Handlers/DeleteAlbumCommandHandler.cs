using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Features.Albums.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Albums.Handlers;

public class DeleteAlbumCommandHandler : IRequestHandler<DeleteAlbumCommand, Result>
{
    private readonly IAlbumRepository _albumRepository;
    private readonly ILogger<DeleteAlbumCommandHandler> _logger;

    public DeleteAlbumCommandHandler(
        IAlbumRepository albumRepository,
        ILogger<DeleteAlbumCommandHandler> logger)
    {
        _albumRepository = albumRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteAlbumCommand request,
        CancellationToken cancellationToken)
    {
        var albumResult = await ValidateAlbumOwnershipAsync(request.UserId, request.AlbumId, cancellationToken);
        if (albumResult.IsFailure)
            return Result.Failure(albumResult.Error);

        await _albumRepository.DeleteAsync(request.AlbumId, cancellationToken);

        _logger.LogInformation("Album deleted: {AlbumId}", request.AlbumId);
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
}
