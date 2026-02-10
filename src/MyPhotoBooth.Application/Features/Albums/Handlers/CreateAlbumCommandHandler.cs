using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Albums.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Albums.Handlers;

public class CreateAlbumCommandHandler : IRequestHandler<CreateAlbumCommand, Result<AlbumResponse>>
{
    private readonly IAlbumRepository _albumRepository;
    private readonly ILogger<CreateAlbumCommandHandler> _logger;

    public CreateAlbumCommandHandler(
        IAlbumRepository albumRepository,
        ILogger<CreateAlbumCommandHandler> logger)
    {
        _albumRepository = albumRepository;
        _logger = logger;
    }

    public async Task<Result<AlbumResponse>> Handle(
        CreateAlbumCommand request,
        CancellationToken cancellationToken)
    {
        var album = new Album
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _albumRepository.AddAsync(album, cancellationToken);

        _logger.LogInformation("Album created: {AlbumId} for user {UserId}", album.Id, request.UserId);

        return Result.Success(new AlbumResponse
        {
            Id = album.Id,
            Name = album.Name,
            Description = album.Description,
            CoverPhotoId = album.CoverPhotoId,
            CreatedAt = album.CreatedAt,
            UpdatedAt = album.UpdatedAt,
            PhotoCount = 0
        });
    }
}
