using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ShareLinksController : ControllerBase
{
    private readonly IShareLinkRepository _shareLinkRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly IAlbumRepository _albumRepository;
    private readonly IPasswordHasher<object> _passwordHasher;

    public ShareLinksController(
        IShareLinkRepository shareLinkRepository,
        IPhotoRepository photoRepository,
        IAlbumRepository albumRepository)
    {
        _shareLinkRepository = shareLinkRepository;
        _photoRepository = photoRepository;
        _albumRepository = albumRepository;
        _passwordHasher = new PasswordHasher<object>();
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User ID not found");

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    [HttpPost]
    public async Task<IActionResult> CreateShareLink([FromBody] CreateShareLinkRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        // Validate ownership
        if (request.Type == ShareLinkType.Photo)
        {
            if (!request.PhotoId.HasValue)
                return BadRequest(new { message = "PhotoId is required for photo shares" });

            var photo = await _photoRepository.GetByIdAsync(request.PhotoId.Value, cancellationToken);
            if (photo == null) return NotFound(new { message = "Photo not found" });
            if (photo.UserId != userId) return Forbid();
        }
        else if (request.Type == ShareLinkType.Album)
        {
            if (!request.AlbumId.HasValue)
                return BadRequest(new { message = "AlbumId is required for album shares" });

            var album = await _albumRepository.GetByIdAsync(request.AlbumId.Value, cancellationToken);
            if (album == null) return NotFound(new { message = "Album not found" });
            if (album.UserId != userId) return Forbid();
        }

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            Token = GenerateToken(),
            UserId = userId,
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

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        return Ok(new ShareLinkResponse
        {
            Id = shareLink.Id,
            Token = shareLink.Token,
            Type = shareLink.Type,
            PhotoId = shareLink.PhotoId,
            AlbumId = shareLink.AlbumId,
            HasPassword = shareLink.PasswordHash != null,
            ExpiresAt = shareLink.ExpiresAt,
            AllowDownload = shareLink.AllowDownload,
            ShareUrl = $"{baseUrl}/shared/{shareLink.Token}",
            IsActive = shareLink.IsActive,
            CreatedAt = shareLink.CreatedAt
        });
    }

    [HttpGet]
    public async Task<IActionResult> ListShareLinks(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var shareLinks = await _shareLinkRepository.GetByUserIdAsync(userId, cancellationToken);

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var response = shareLinks.Select(sl => new ShareLinkResponse
        {
            Id = sl.Id,
            Token = sl.Token,
            Type = sl.Type,
            PhotoId = sl.PhotoId,
            AlbumId = sl.AlbumId,
            TargetName = sl.Type == ShareLinkType.Photo
                ? sl.Photo?.OriginalFileName
                : sl.Album?.Name,
            HasPassword = sl.PasswordHash != null,
            ExpiresAt = sl.ExpiresAt,
            AllowDownload = sl.AllowDownload,
            ShareUrl = $"{baseUrl}/shared/{sl.Token}",
            IsActive = sl.IsActive,
            CreatedAt = sl.CreatedAt
        }).ToList();

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RevokeShareLink(Guid id, CancellationToken cancellationToken)
    {
        var shareLink = await _shareLinkRepository.GetByIdAsync(id, cancellationToken);

        if (shareLink == null)
            return NotFound();

        if (shareLink.UserId != GetUserId())
            return Forbid();

        shareLink.RevokedAt = DateTime.UtcNow;
        await _shareLinkRepository.UpdateAsync(shareLink, cancellationToken);

        return NoContent();
    }
}
