using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SharedController : ControllerBase
{
    private readonly IShareLinkRepository _shareLinkRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IPasswordHasher<object> _passwordHasher;

    public SharedController(
        IShareLinkRepository shareLinkRepository,
        IFileStorageService fileStorageService)
    {
        _shareLinkRepository = shareLinkRepository;
        _fileStorageService = fileStorageService;
        _passwordHasher = new PasswordHasher<object>();
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetShareMetadata(string token, CancellationToken cancellationToken)
    {
        var shareLink = await _shareLinkRepository.GetByTokenAsync(token, cancellationToken);

        if (shareLink == null)
            return NotFound(new { message = "Share link not found" });

        return Ok(new ShareMetadataResponse
        {
            Type = shareLink.Type,
            HasPassword = shareLink.PasswordHash != null,
            IsExpired = shareLink.IsExpired,
            IsActive = shareLink.IsActive
        });
    }

    [HttpPost("{token}/access")]
    public async Task<IActionResult> AccessSharedContent(string token, [FromBody] VerifySharePasswordRequest request, CancellationToken cancellationToken)
    {
        var shareLink = await _shareLinkRepository.GetByTokenAsync(token, cancellationToken);

        if (shareLink == null)
            return NotFound(new { message = "Share link not found" });

        if (!shareLink.IsActive)
            return Gone();

        // Verify password if required
        if (shareLink.PasswordHash != null)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                return Unauthorized(new { message = "Password required" });

            var result = _passwordHasher.VerifyHashedPassword(new object(), shareLink.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Incorrect password" });
        }

        // Build response based on type
        if (shareLink.Type == ShareLinkType.Photo && shareLink.Photo != null)
        {
            return Ok(new SharedContentResponse
            {
                Type = ShareLinkType.Photo,
                Photo = new SharedPhotoResponse
                {
                    Id = shareLink.Photo.Id,
                    FileName = shareLink.Photo.OriginalFileName,
                    Width = shareLink.Photo.Width,
                    Height = shareLink.Photo.Height,
                    CapturedAt = shareLink.Photo.CapturedAt,
                    UploadedAt = shareLink.Photo.UploadedAt,
                    Description = shareLink.Photo.Description,
                    AllowDownload = shareLink.AllowDownload
                }
            });
        }
        else if (shareLink.Type == ShareLinkType.Album && shareLink.Album != null)
        {
            var photos = shareLink.Album.AlbumPhotos
                .OrderBy(ap => ap.SortOrder)
                .Select(ap => new SharedPhotoResponse
                {
                    Id = ap.Photo.Id,
                    FileName = ap.Photo.OriginalFileName,
                    Width = ap.Photo.Width,
                    Height = ap.Photo.Height,
                    CapturedAt = ap.Photo.CapturedAt,
                    UploadedAt = ap.Photo.UploadedAt,
                    Description = ap.Photo.Description,
                    AllowDownload = shareLink.AllowDownload
                }).ToList();

            return Ok(new SharedContentResponse
            {
                Type = ShareLinkType.Album,
                Album = new SharedAlbumResponse
                {
                    Name = shareLink.Album.Name,
                    Description = shareLink.Album.Description,
                    AllowDownload = shareLink.AllowDownload,
                    Photos = photos
                }
            });
        }

        return NotFound(new { message = "Shared content not found" });
    }

    [HttpGet("{token}/photos/{photoId}/thumbnail")]
    public async Task<IActionResult> GetSharedThumbnail(string token, Guid photoId, CancellationToken cancellationToken)
    {
        var photo = await ValidateAndGetPhoto(token, photoId, cancellationToken);
        if (photo == null)
            return NotFound();

        var stream = await _fileStorageService.GetFileStreamAsync(photo.ThumbnailPath, cancellationToken);
        if (stream == null)
            return NotFound();

        return File(stream, "image/jpeg");
    }

    [HttpGet("{token}/photos/{photoId}/file")]
    public async Task<IActionResult> GetSharedFile(string token, Guid photoId, CancellationToken cancellationToken)
    {
        var shareLink = await _shareLinkRepository.GetByTokenAsync(token, cancellationToken);

        if (shareLink == null || !shareLink.IsActive)
            return NotFound();

        if (!shareLink.AllowDownload)
            return Forbid();

        var photo = GetPhotoFromShareLink(shareLink, photoId);
        if (photo == null)
            return NotFound();

        var stream = await _fileStorageService.GetFileStreamAsync(photo.FilePath, cancellationToken);
        if (stream == null)
            return NotFound();

        return File(stream, photo.ContentType, photo.OriginalFileName);
    }

    private async Task<Photo?> ValidateAndGetPhoto(string token, Guid photoId, CancellationToken cancellationToken)
    {
        var shareLink = await _shareLinkRepository.GetByTokenAsync(token, cancellationToken);

        if (shareLink == null || !shareLink.IsActive)
            return null;

        return GetPhotoFromShareLink(shareLink, photoId);
    }

    private static Photo? GetPhotoFromShareLink(ShareLink shareLink, Guid photoId)
    {
        if (shareLink.Type == ShareLinkType.Photo)
        {
            if (shareLink.PhotoId == photoId && shareLink.Photo != null)
                return shareLink.Photo;
        }
        else if (shareLink.Type == ShareLinkType.Album && shareLink.Album != null)
        {
            var albumPhoto = shareLink.Album.AlbumPhotos
                .FirstOrDefault(ap => ap.PhotoId == photoId);
            return albumPhoto?.Photo;
        }

        return null;
    }

    private ObjectResult Gone()
    {
        return StatusCode(410, new { message = "This share link has expired or been revoked" });
    }
}
