using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagRepository _tagRepository;

    public TagsController(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) 
        ?? throw new UnauthorizedAccessException("User ID not found");

    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        // Check if tag already exists
        var existingTag = await _tagRepository.GetByNameAsync(request.Name, userId, cancellationToken);
        if (existingTag != null)
        {
            return Ok(new TagResponse
            {
                Id = existingTag.Id,
                Name = existingTag.Name,
                CreatedAt = existingTag.CreatedAt
            });
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _tagRepository.AddAsync(tag, cancellationToken);

        return Ok(new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            CreatedAt = tag.CreatedAt
        });
    }

    [HttpGet]
    public async Task<IActionResult> ListTags(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var tags = await _tagRepository.GetByUserIdAsync(userId, cancellationToken);

        var tagList = tags.Select(t => new TagResponse
        {
            Id = t.Id,
            Name = t.Name,
            CreatedAt = t.CreatedAt
        }).ToList();

        return Ok(tagList);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchTags([FromQuery] string query, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var tags = await _tagRepository.SearchAsync(query, userId, cancellationToken);

        var tagList = tags.Select(t => new TagResponse
        {
            Id = t.Id,
            Name = t.Name,
            CreatedAt = t.CreatedAt
        }).ToList();

        return Ok(tagList);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(Guid id, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);
        
        if (tag == null)
        {
            return NotFound();
        }

        if (tag.UserId != GetUserId())
        {
            return Forbid();
        }

        await _tagRepository.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
