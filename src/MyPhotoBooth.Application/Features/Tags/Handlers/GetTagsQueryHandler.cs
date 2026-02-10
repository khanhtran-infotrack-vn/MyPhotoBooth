using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Tags.Queries;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Tags.Handlers;

public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, Result<List<TagResponse>>>
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<GetTagsQueryHandler> _logger;

    public GetTagsQueryHandler(
        ITagRepository tagRepository,
        ILogger<GetTagsQueryHandler> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    public async Task<Result<List<TagResponse>>> Handle(
        GetTagsQuery request,
        CancellationToken cancellationToken)
    {
        var tags = await _tagRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        var tagList = tags.Select(t => new TagResponse
        {
            Id = t.Id,
            Name = t.Name,
            CreatedAt = t.CreatedAt
        }).ToList();

        return Result.Success(tagList);
    }
}
