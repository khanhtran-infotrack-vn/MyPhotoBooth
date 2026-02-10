using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBooth.API.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Albums.Commands;
using MyPhotoBooth.Application.Features.Albums.Queries;
using System.Security.Claims;

namespace MyPhotoBooth.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AlbumsController : ControllerBase
{
    private readonly ISender _sender;

    public AlbumsController(ISender sender)
    {
        _sender = sender;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    [HttpPost]
    public async Task<IActionResult> CreateAlbum([FromBody] CreateAlbumRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAlbumCommand(request.Name, request.Description, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { message = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> ListAlbums(CancellationToken cancellationToken)
    {
        var query = new GetAlbumsQuery(GetUserId());
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { message = result.Error });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAlbum(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAlbumQuery(id, GetUserId());
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return result.ToHttpResponse();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAlbum(Guid id, [FromBody] UpdateAlbumRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateAlbumCommand(id, request.Name, request.Description, request.CoverPhotoId, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return result.ToHttpResponse();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAlbum(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteAlbumCommand(id, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return result.ToHttpResponse();
    }

    [HttpPost("{id}/photos")]
    public async Task<IActionResult> AddPhotosToAlbum(Guid id, [FromBody] List<Guid> photoIds, CancellationToken cancellationToken)
    {
        var command = new AddPhotosToAlbumCommand(id, photoIds, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return result.ToHttpResponse();
    }

    [HttpDelete("{id}/photos")]
    public async Task<IActionResult> RemovePhotosFromAlbum(Guid id, [FromBody] List<Guid> photoIds, CancellationToken cancellationToken)
    {
        var command = new RemovePhotosFromAlbumCommand(id, photoIds, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return result.ToHttpResponse();
    }

    [HttpGet("{id}/photos")]
    public async Task<IActionResult> GetAlbumPhotos(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAlbumPhotosQuery(id, GetUserId());
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return result.ToHttpResponse();
    }
}
