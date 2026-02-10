using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBooth.API.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.ShareLinks.Commands;
using MyPhotoBooth.Application.Features.ShareLinks.Queries;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SharedController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IFileStorageService _fileStorageService;

    public SharedController(ISender sender, IFileStorageService fileStorageService)
    {
        _sender = sender;
        _fileStorageService = fileStorageService;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetShareMetadata(string token, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetShareLinkByTokenQuery(token), cancellationToken);

        if (result.IsSuccess)
            return Ok(new ShareMetadataResponse
            {
                Type = result.Value.Type,
                HasPassword = result.Value.PasswordHash != null,
                IsExpired = result.Value.IsExpired,
                IsActive = result.Value.IsActive
            });
        return NotFound(new { message = result.Error });
    }

    [HttpPost("{token}/access")]
    public async Task<IActionResult> AccessSharedContent(string token, [FromBody] VerifySharePasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ValidateShareLinkQuery(token, request.Password);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        var error = result.Error;
        if (error.Contains("revoked", StringComparison.OrdinalIgnoreCase) || error.Contains("expired", StringComparison.OrdinalIgnoreCase))
            return StatusCode(410, new { message = error });
        if (error.Contains("password", StringComparison.OrdinalIgnoreCase))
            return Unauthorized(new { message = error });
        return NotFound(new { message = error });
    }

    [HttpGet("{token}/photos/{photoId}/thumbnail")]
    public async Task<IActionResult> GetSharedThumbnail(string token, Guid photoId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSharedPhotoQuery(token, photoId), cancellationToken);

        if (result.IsFailure)
            return NotFound();

        var photo = result.Value;
        var stream = await _fileStorageService.GetFileStreamAsync(photo.ThumbnailPath, cancellationToken);
        if (stream == null)
            return NotFound();
        return File(stream, "image/jpeg");
    }

    [HttpGet("{token}/photos/{photoId}/file")]
    public async Task<IActionResult> GetSharedFile(string token, Guid photoId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSharedFileQuery(token, photoId), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("not allowed", StringComparison.OrdinalIgnoreCase))
                return Forbid();
            return NotFound();
        }

        var (photo, allowDownload) = result.Value;
        var stream = await _fileStorageService.GetFileStreamAsync(photo.FilePath, cancellationToken);
        if (stream == null)
            return NotFound();
        return File(stream, photo.ContentType, photo.OriginalFileName);
    }
}
