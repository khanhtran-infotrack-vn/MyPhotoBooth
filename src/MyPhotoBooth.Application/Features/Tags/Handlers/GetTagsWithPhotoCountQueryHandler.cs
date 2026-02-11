using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Tags.Queries;
using MyPhotoBooth.Application.Interfaces;
using CSharpFunctionalExtensions;

namespace MyPhotoBooth.Application.Features.Tags.Handlers;

public class GetTagsWithPhotoCountQueryHandler : IRequestHandler<GetTagsWithPhotoCountQuery, Result<List<TagWithPhotoCountResponse>>>
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<GetTagsWithPhotoCountQueryHandler> _logger;

    public GetTagsWithPhotoCountQueryHandler(
        ITagRepository tagRepository,
        ILogger<GetTagsWithPhotoCountQueryHandler> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    public async Task<Result<List<TagWithPhotoCountResponse>>> Handle(
        GetTagsWithPhotoCountQuery request,
        CancellationToken cancellationToken)
    {
        var tags = await _tagRepository.GetTagsWithPhotoCountAsync(request.UserId, cancellationToken);

        _logger.LogInformation("Retrieved {Count} tags with photo counts for user {UserId}", tags.Count, request.UserId);

        return Result.Success(tags);
    }
}
