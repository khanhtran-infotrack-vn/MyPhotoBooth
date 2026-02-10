using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Tags.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Tags.Handlers;

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Result<TagResponse>>
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<CreateTagCommandHandler> _logger;

    public CreateTagCommandHandler(
        ITagRepository tagRepository,
        ILogger<CreateTagCommandHandler> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    public async Task<Result<TagResponse>> Handle(
        CreateTagCommand request,
        CancellationToken cancellationToken)
    {
        var existingTag = await _tagRepository.GetByNameAsync(request.Name, request.UserId, cancellationToken);
        if (existingTag != null)
        {
            _logger.LogInformation("Tag already exists: {TagId} for user {UserId}", existingTag.Id, request.UserId);
            return Result.Success(new TagResponse
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
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await _tagRepository.AddAsync(tag, cancellationToken);

        _logger.LogInformation("Tag created: {TagId} for user {UserId}", tag.Id, request.UserId);

        return Result.Success(new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            CreatedAt = tag.CreatedAt
        });
    }
}
