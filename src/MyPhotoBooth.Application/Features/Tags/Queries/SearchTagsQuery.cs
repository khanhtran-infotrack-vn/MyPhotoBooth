using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Tags.Queries;

public record SearchTagsQuery(
    string SearchQuery,
    string UserId
) : IQuery<List<TagResponse>>;
