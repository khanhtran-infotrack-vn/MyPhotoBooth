using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Tags.Queries;

public record GetTagsWithPhotoCountQuery(string UserId) : IQuery<List<TagWithPhotoCountResponse>>;
