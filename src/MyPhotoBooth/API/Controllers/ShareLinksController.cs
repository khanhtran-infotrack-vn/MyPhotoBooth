using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBooth.API.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.ShareLinks.Commands;
using MyPhotoBooth.Application.Features.ShareLinks.Queries;
using System.Security.Claims;

namespace MyPhotoBooth.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ShareLinksController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ShareLinksController(ISender sender, IHttpContextAccessor httpContextAccessor)
    {
        _sender = sender;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    [HttpPost]
    public async Task<IActionResult> CreateShareLink([FromBody] CreateShareLinkRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var scheme = _httpContextAccessor.HttpContext?.Request.Scheme ?? "https";
        var host = _httpContextAccessor.HttpContext?.Request.Host.Value ?? "localhost";
        var baseUrl = $"{scheme}://{host}";

        var command = new CreateShareLinkCommand(
            request.Type,
            request.PhotoId,
            request.AlbumId,
            request.Password,
            request.ExpiresAt,
            request.AllowDownload,
            userId,
            baseUrl
        );

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { message = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> ListShareLinks(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var scheme = _httpContextAccessor.HttpContext?.Request.Scheme ?? "https";
        var host = _httpContextAccessor.HttpContext?.Request.Host.Value ?? "localhost";
        var baseUrl = $"{scheme}://{host}";

        var query = new GetShareLinksQuery(userId, baseUrl);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { message = result.Error });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RevokeShareLink(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteShareLinkCommand(id, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return result.ToHttpResponse();
    }
}
