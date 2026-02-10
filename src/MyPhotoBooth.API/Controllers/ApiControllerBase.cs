using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBooth.Application.Common;
using System.Security.Claims;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException(Errors.General.Unauthorized);

    protected IActionResult HandleResultError(string error)
    {
        return error switch
        {
            var e when e.Contains("not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { message = error }),
            var e when e.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) || e.Contains("access", StringComparison.OrdinalIgnoreCase) => Forbid(),
            var e when e.Contains("expired", StringComparison.OrdinalIgnoreCase) => Unauthorized(new { message = error }),
            var e when e.Contains("invalid", StringComparison.OrdinalIgnoreCase) => BadRequest(new { message = error }),
            _ => BadRequest(new { message = error })
        };
    }
}
