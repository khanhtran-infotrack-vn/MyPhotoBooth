using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.ShareLinks.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using System.Security.Cryptography;

namespace MyPhotoBooth.Application.Features.ShareLinks.Handlers;

public class CreateShareLinkCommandHandler : IRequestHandler<CreateShareLinkCommand, Result<ShareLinkResponse>>
{
    private readonly IShareLinkRepository _shareLinkRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly IAlbumRepository _albumRepository;
    private readonly IPasswordHasher<object> _passwordHasher;
    private readonly ILogger<CreateShareLinkCommandHandler> _logger;

    public CreateShareLinkCommandHandler(
        IShareLinkRepository shareLinkRepository,
        IPhotoRepository photoRepository,
        IAlbumRepository albumRepository,
        ILogger<CreateShareLinkCommandHandler> logger)
    {
        _shareLinkRepository = shareLinkRepository;
        _photoRepository = photoRepository;
        _albumRepository = albumRepository;
        _passwordHasher = new PasswordHasher<object>();
        _logger = logger;
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public async Task<Result<ShareLinkResponse>> Handle(
        CreateShareLinkCommand request,
        CancellationToken cancellationToken)
    {
        // Validate ownership based on type
        if (request.Type == ShareLinkType.Photo)
        {
            if (!request.PhotoId.HasValue)
                return Result.Failure<ShareLinkResponse>(Errors.ShareLinks.PhotoIdRequired);

            var photoResult = await ValidatePhotoOwnershipAsync(request.UserId, request.PhotoId.Value, cancellationToken);
            if (photoResult.IsFailure)
                return Result.Failure<ShareLinkResponse>(photoResult.Error);
        }
        else if (request.Type == ShareLinkType.Album)
        {
            if (!request.AlbumId.HasValue)
                return Result.Failure<ShareLinkResponse>(Errors.ShareLinks.AlbumIdRequired);

            var albumResult = await ValidateAlbumOwnershipAsync(request.UserId, request.AlbumId.Value, cancellationToken);
            if (albumResult.IsFailure)
                return Result.Failure<ShareLinkResponse>(albumResult.Error);
        }

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            Token = GenerateToken(),
            UserId = request.UserId,
            Type = request.Type,
            PhotoId = request.Type == ShareLinkType.Photo ? request.PhotoId : null,
            AlbumId = request.Type == ShareLinkType.Album ? request.AlbumId : null,
            ExpiresAt = request.ExpiresAt?.ToUniversalTime(),
            AllowDownload = request.AllowDownload,
            CreatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            shareLink.PasswordHash = _passwordHasher.HashPassword(new object(), request.Password);
        }

        await _shareLinkRepository.AddAsync(shareLink, cancellationToken);

        _logger.LogInformation("Share link created: {ShareLinkId} for user {UserId}", shareLink.Id, request.UserId);

        return Result.Success(new ShareLinkResponse
        {
            Id = shareLink.Id,
            Token = shareLink.Token,
            Type = shareLink.Type,
            PhotoId = shareLink.PhotoId,
            AlbumId = shareLink.AlbumId,
            HasPassword = shareLink.PasswordHash != null,
            ExpiresAt = shareLink.ExpiresAt,
            AllowDownload = shareLink.AllowDownload,
            ShareUrl = $"{request.BaseUrl}/shared/{shareLink.Token}",
            IsActive = shareLink.IsActive,
            CreatedAt = shareLink.CreatedAt
        });
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
