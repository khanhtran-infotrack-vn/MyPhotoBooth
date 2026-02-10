using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace MyPhotoBooth.API.Common;

public static class ResultExtensions
{
    public static IActionResult ToHttpResponse<TResult>(this Result<TResult> result)
    {
        if (result.IsSuccess) return new OkObjectResult(result.Value);

        return result.Error switch
        {
            var e when e.Contains("not found", StringComparison.OrdinalIgnoreCase)
                => new NotFoundObjectResult(new { message = result.Error }),
            var e when e.Contains("unauthorized", StringComparison.OrdinalIgnoreCase)
                => new UnauthorizedObjectResult(new { message = result.Error }),
            _ => new BadRequestObjectResult(new { message = result.Error })
        };
    }

    public static IActionResult ToHttpResponse(this Result result)
    {
        if (result.IsSuccess) return new NoContentResult();

        return result.Error switch
        {
            var e when e.Contains("not found", StringComparison.OrdinalIgnoreCase)
                => new NotFoundObjectResult(new { message = result.Error }),
            var e when e.Contains("unauthorized", StringComparison.OrdinalIgnoreCase)
                => new UnauthorizedObjectResult(new { message = result.Error }),
            _ => new BadRequestObjectResult(new { message = result.Error })
        };
    }

    public static IActionResult ToCreatedResponse<TResult>(this Result<TResult> result, string location)
    {
        if (result.IsSuccess) return new CreatedResult(location, result.Value);

        return result.Error switch
        {
            var e when e.Contains("not found", StringComparison.OrdinalIgnoreCase)
                => new NotFoundObjectResult(new { message = result.Error }),
            var e when e.Contains("unauthorized", StringComparison.OrdinalIgnoreCase)
                => new UnauthorizedObjectResult(new { message = result.Error }),
            _ => new BadRequestObjectResult(new { message = result.Error })
        };
    }
}
