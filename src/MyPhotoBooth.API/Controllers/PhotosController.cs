using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using MyPhotoBooth.API.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Features.Photos.Queries;

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
    public async Task<IActionResult> ListPhotos([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = new GetPhotosQuery(page, pageSize, null, null, GetUserId());
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
}
