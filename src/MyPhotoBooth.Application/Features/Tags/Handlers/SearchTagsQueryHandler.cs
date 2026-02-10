using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Tags.Queries;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Tags.Handlers;

public class SearchTagsQueryHandler : IRequestHandler<SearchTagsQuery, Result<List<TagResponse>>>
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<SearchTagsQueryHandler> _logger;

    public SearchTagsQueryHandler(
        ITagRepository tagRepository,
        ILogger<SearchTagsQueryHandler> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    public async Task<Result<List<TagResponse>>> Handle(
        SearchTagsQuery request,
        CancellationToken cancellationToken)
    {
        var tags = await _tagRepository.SearchAsync(request.SearchQuery, request.UserId, cancellationToken);

        var tagList = tags.Select(t => new TagResponse
        {
            Id = t.Id,
            Name = t.Name,
            CreatedAt = t.CreatedAt
        }).ToList();

        return Result.Success(tagList);
    }
}
