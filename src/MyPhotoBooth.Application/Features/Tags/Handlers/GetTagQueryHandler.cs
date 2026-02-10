using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Tags.Queries;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Tags.Handlers;

public class GetTagQueryHandler : IRequestHandler<GetTagQuery, Result<TagResponse>>
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<GetTagQueryHandler> _logger;

    public GetTagQueryHandler(
        ITagRepository tagRepository,
        ILogger<GetTagQueryHandler> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    public async Task<Result<TagResponse>> Handle(
        GetTagQuery request,
        CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(request.TagId, cancellationToken);

        if (tag == null)
            return Result.Failure<TagResponse>(Errors.Tags.NotFound);

        if (tag.UserId != request.UserId)
            return Result.Failure<TagResponse>(Errors.General.Unauthorized);

        return Result.Success(new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            CreatedAt = tag.CreatedAt
        });
    }
}
