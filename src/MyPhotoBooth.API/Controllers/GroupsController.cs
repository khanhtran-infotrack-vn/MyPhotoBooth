using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBooth.API.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Groups.Commands;
using MyPhotoBooth.Application.Features.Groups.Queries;
using System.Security.Claims;

namespace MyPhotoBooth.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly ISender _sender;

    public GroupsController(ISender sender)
    {
        _sender = sender;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateGroupCommand(request.Name, request.Description, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { message = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> ListGroups(CancellationToken cancellationToken)
    {
        var query = new GetGroupsQuery(GetUserId());
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { message = result.Error });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGroup(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetGroupQuery(id, GetUserId());
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return result.ToHttpResponse();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] UpdateGroupRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateGroupCommand(id, request.Name, request.Description, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return result.ToHttpResponse();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGroup(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteGroupCommand(id, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return result.ToHttpResponse();
    }

    // Membership endpoints

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddGroupMemberRequest request, CancellationToken cancellationToken)
    {
        var command = new AddGroupMemberCommand(id, request.Email, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { message = result.Error });
    }

    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetMembers(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetGroupMembersQuery(id, GetUserId());
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { message = result.Error });
    }

    [HttpDelete("{id}/members/{memberUserId}")]
    public async Task<IActionResult> RemoveMember(Guid id, string memberUserId, CancellationToken cancellationToken)
    {
        var command = new RemoveGroupMemberCommand(id, memberUserId, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return BadRequest(new { message = result.Error });
    }

    [HttpPost("{id}/leave")]
    public async Task<IActionResult> LeaveGroup(Guid id, CancellationToken cancellationToken)
    {
        var command = new LeaveGroupCommand(id, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return BadRequest(new { message = result.Error });
    }

    [HttpPost("{id}/transfer-ownership")]
    public async Task<IActionResult> TransferOwnership(Guid id, [FromBody] TransferOwnershipRequest request, CancellationToken cancellationToken)
    {
        var command = new TransferOwnershipCommand(id, request.UserId, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { message = result.Error });
    }

    // Content sharing endpoints

    [HttpPost("{id}/photos")]
    public async Task<IActionResult> SharePhoto(Guid id, [FromBody] ShareContentToGroupRequest request, CancellationToken cancellationToken)
    {
        var command = new SharePhotoToGroupCommand(id, request.PhotoId ?? Guid.Empty, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return BadRequest(new { message = result.Error });
    }

    [HttpDelete("{id}/photos/{photoId}")]
    public async Task<IActionResult> RemovePhoto(Guid id, Guid photoId, CancellationToken cancellationToken)
    {
        var command = new RemovePhotoFromGroupCommand(id, photoId, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return BadRequest(new { message = result.Error });
    }

    [HttpGet("{id}/photos")]
    public async Task<IActionResult> GetPhotos(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetGroupPhotosQuery(id, GetUserId());
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { message = result.Error });
    }

    [HttpPost("{id}/albums")]
    public async Task<IActionResult> ShareAlbum(Guid id, [FromBody] ShareContentToGroupRequest request, CancellationToken cancellationToken)
    {
        var command = new ShareAlbumToGroupCommand(id, request.AlbumId ?? Guid.Empty, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return BadRequest(new { message = result.Error });
    }

    [HttpDelete("{id}/albums/{albumId}")]
    public async Task<IActionResult> RemoveAlbum(Guid id, Guid albumId, CancellationToken cancellationToken)
    {
        var command = new RemoveAlbumFromGroupCommand(id, albumId, GetUserId());
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return BadRequest(new { message = result.Error });
    }

    [HttpGet("{id}/albums")]
    public async Task<IActionResult> GetAlbums(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetGroupAlbumsQuery(id, GetUserId());
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { message = result.Error });
    }
}
