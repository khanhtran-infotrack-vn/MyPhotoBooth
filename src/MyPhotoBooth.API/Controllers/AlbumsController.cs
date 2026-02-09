using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AlbumsController : ControllerBase
{
    private readonly IAlbumRepository _albumRepository;

    public AlbumsController(IAlbumRepository albumRepository)
    {
        _albumRepository = albumRepository;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) 
        ?? throw new UnauthorizedAccessException("User ID not found");

    [HttpPost]
    public async Task<IActionResult> CreateAlbum([FromBody] CreateAlbumRequest request, CancellationToken cancellationToken)
    {
        var album = new Album
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            UserId = GetUserId(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _albumRepository.AddAsync(album, cancellationToken);

        return Ok(new AlbumResponse
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

    [HttpGet]
    public async Task<IActionResult> ListAlbums(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var albums = await _albumRepository.GetByUserIdAsync(userId, cancellationToken);

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

        return Ok(albumList);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAlbum(Guid id, CancellationToken cancellationToken)
    {
        var album = await _albumRepository.GetByIdAsync(id, cancellationToken);
        
        if (album == null)
        {
            return NotFound();
        }

        if (album.UserId != GetUserId())
        {
            return Forbid();
        }

        return Ok(new AlbumDetailsResponse
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
                CapturedAt = ap.Photo.CapturedAt,
                UploadedAt = ap.Photo.UploadedAt,
                ThumbnailPath = ap.Photo.ThumbnailPath
            }).ToList()
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAlbum(Guid id, [FromBody] UpdateAlbumRequest request, CancellationToken cancellationToken)
    {
        var album = await _albumRepository.GetByIdAsync(id, cancellationToken);
        
        if (album == null)
        {
            return NotFound();
        }

        if (album.UserId != GetUserId())
        {
            return Forbid();
        }

        album.Name = request.Name;
        album.Description = request.Description;
        album.CoverPhotoId = request.CoverPhotoId;
        album.UpdatedAt = DateTime.UtcNow;

        await _albumRepository.UpdateAsync(album, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAlbum(Guid id, CancellationToken cancellationToken)
    {
        var album = await _albumRepository.GetByIdAsync(id, cancellationToken);
        
        if (album == null)
        {
            return NotFound();
        }

        if (album.UserId != GetUserId())
        {
            return Forbid();
        }

        await _albumRepository.DeleteAsync(id, cancellationToken);

        return NoContent();
    }

    [HttpPost("{id}/photos")]
    public async Task<IActionResult> AddPhotoToAlbum(Guid id, [FromBody] AddPhotoToAlbumRequest request, CancellationToken cancellationToken)
    {
        var album = await _albumRepository.GetByIdAsync(id, cancellationToken);
        
        if (album == null)
        {
            return NotFound();
        }

        if (album.UserId != GetUserId())
        {
            return Forbid();
        }

        var sortOrder = album.AlbumPhotos.Count;
        await _albumRepository.AddPhotoToAlbumAsync(id, request.PhotoId, sortOrder, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}/photos/{photoId}")]
    public async Task<IActionResult> RemovePhotoFromAlbum(Guid id, Guid photoId, CancellationToken cancellationToken)
    {
        var album = await _albumRepository.GetByIdAsync(id, cancellationToken);
        
        if (album == null)
        {
            return NotFound();
        }

        if (album.UserId != GetUserId())
        {
            return Forbid();
        }

        await _albumRepository.RemovePhotoFromAlbumAsync(id, photoId, cancellationToken);

        return NoContent();
    }
}
