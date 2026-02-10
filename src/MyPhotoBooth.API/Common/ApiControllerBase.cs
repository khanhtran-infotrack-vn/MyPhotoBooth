using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyPhotoBooth.API.Common;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ApiControllerBase : ControllerBase
{
    protected ISender Mediator => HttpContext.RequestServices.GetRequiredService<ISender>();
}
