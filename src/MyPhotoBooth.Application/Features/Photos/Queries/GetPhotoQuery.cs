using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Queries;

public record GetPhotoQuery(
    Guid PhotoId,
    string UserId
) : IQuery<PhotoDetailsResponse>;
