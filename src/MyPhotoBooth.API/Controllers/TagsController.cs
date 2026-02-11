using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBooth.API.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Tags.Commands;
using MyPhotoBooth.Application.Features.Tags.Queries;
using System.Security.Claims;

namespace MyPhotoBooth.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ISender _sender;

    public TagsController(ISender sender)
    {
        _sender = sender;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTagCommand(request.Name, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { message = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> ListTags(CancellationToken cancellationToken)
    {
        var query = new GetTagsQuery(GetUserId());
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { message = result.Error });
    }

    [HttpGet("with-count")]
    public async Task<IActionResult> GetTagsWithCount(CancellationToken cancellationToken)
    {
        var query = new GetTagsWithPhotoCountQuery(GetUserId());
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { message = result.Error });
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchTags([FromQuery] string query, CancellationToken cancellationToken)
    {
        var queryObj = new SearchTagsQuery(query, GetUserId());
        var result = await _sender.Send(queryObj, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { message = result.Error });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteTagCommand(id, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return result.ToHttpResponse();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTag(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetTagQuery(id, GetUserId());
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return result.ToHttpResponse();
    }

    [HttpGet("{id}/photos")]
    public async Task<IActionResult> GetTagPhotos(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTagPhotosQuery(id, GetUserId(), page, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return result.ToHttpResponse();
    }

    [HttpDelete("{tagId}/photos/{photoId}")]
    public async Task<IActionResult> RemoveTagFromPhoto(
        Guid tagId,
        Guid photoId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveTagFromPhotoCommand(photoId, tagId, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return result.ToHttpResponse();
    }
}
