using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using MyPhotoBooth.API.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Features.Photos.Queries;
using MyPhotoBooth.Application.Features.Tags.Commands;

namespace MyPhotoBooth.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PhotosController : ControllerBase
{
    private readonly ISender _mediator;

    public PhotosController(ISender mediator)
    {
        _mediator = mediator;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    [HttpPost]
    public async Task<IActionResult> UploadPhoto(IFormFile file, [FromForm] string? description, CancellationToken cancellationToken)
    {
        var command = new UploadPhotoCommand(file, description, GetUserId());
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpGet]
    public async Task<IActionResult> ListPhotos([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] PhotoSortOrder sortBy = PhotoSortOrder.UploadedAtDesc, CancellationToken cancellationToken = default)
    {
        var query = new GetPhotosQuery(page, pageSize, null, null, GetUserId(), sortBy);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPhoto(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPhotoQuery(id, GetUserId());
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePhoto(Guid id, [FromBody] UpdatePhotoRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdatePhotoCommand(id, request.Description, GetUserId());
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePhoto(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeletePhotoCommand(id, GetUserId());
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpGet("{id}/file")]
    public async Task<IActionResult> GetPhotoFile(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPhotoFileQuery(id, GetUserId());
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess)
            return File(result.Value.Stream, result.Value.ContentType, result.Value.FileName);

        return result.ToHttpResponse();
    }

    [HttpGet("{id}/thumbnail")]
    public async Task<IActionResult> GetPhotoThumbnail(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPhotoThumbnailQuery(id, GetUserId());
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess)
            return File(result.Value, "image/jpeg");

        return result.ToHttpResponse();
    }

    [HttpGet("timeline")]
    public async Task<IActionResult> GetTimeline([FromQuery] int? year, [FromQuery] int? month, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = new GetTimelineQuery(year, month, page, pageSize, GetUserId());
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpPost("{id}/favorite")]
    public async Task<IActionResult> ToggleFavorite(Guid id, CancellationToken cancellationToken)
    {
        var command = new ToggleFavoritePhotoCommand(id, GetUserId());
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpPost("{id}/tags")]
    public async Task<IActionResult> AddTagsToPhoto(Guid id, [FromBody] List<Guid> tagIds, CancellationToken cancellationToken)
    {
        var command = new AddTagsToPhotoCommand(id, tagIds, GetUserId());
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpDelete("{photoId}/tags/{tagId}")]
    public async Task<IActionResult> RemoveTagFromPhoto(Guid photoId, Guid tagId, CancellationToken cancellationToken)
    {
        var command = new RemoveTagFromPhotoCommand(photoId, tagId, GetUserId());
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpGet("favorites")]
    public async Task<IActionResult> GetFavorites([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = new GetFavoritePhotosQuery(GetUserId(), page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchPhotos([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = new SearchPhotosQuery(q, GetUserId(), page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToHttpResponse();
    }

    // Bulk Operations

    [HttpPost("bulk/delete")]
    public async Task<IActionResult> BulkDeletePhotos([FromBody] BulkOperationRequestDto request, CancellationToken cancellationToken)
    {
        var command = new BulkDeletePhotosCommand(request.PhotoIds, GetUserId());
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpPost("bulk/favorite")]
    public async Task<IActionResult> BulkToggleFavoritePhotos([FromBody] BulkToggleFavoriteRequestDto request, CancellationToken cancellationToken)
    {
        var command = new BulkToggleFavoritePhotosCommand(request.PhotoIds, GetUserId(), request.Favorite);
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpPost("bulk/add-to-album")]
    public async Task<IActionResult> BulkAddPhotosToAlbum([FromBody] BulkAlbumOperationRequestDto request, CancellationToken cancellationToken)
    {
        var command = new BulkAddPhotosToAlbumCommand(request.PhotoIds, request.AlbumId, GetUserId());
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpPost("bulk/remove-from-album")]
    public async Task<IActionResult> BulkRemovePhotosFromAlbum([FromBody] BulkAlbumOperationRequestDto request, CancellationToken cancellationToken)
    {
        var command = new BulkRemovePhotosFromAlbumCommand(request.PhotoIds, request.AlbumId, GetUserId());
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpGet("bulk/download")]
    public async Task<IActionResult> BulkDownloadPhotos([FromQuery] string photoIds, CancellationToken cancellationToken)
    {
        var ids = photoIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Guid.Parse)
            .ToList();

        var query = new BulkDownloadPhotosQuery(ids, GetUserId());
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess)
        {
            return File(result.Value.FileContents, result.Value.ContentType, result.Value.FileName);
        }

        return result.ToHttpResponse();
    }
}
