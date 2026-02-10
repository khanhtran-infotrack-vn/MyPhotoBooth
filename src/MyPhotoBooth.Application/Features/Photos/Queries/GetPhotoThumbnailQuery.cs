using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Queries;

public record GetPhotoThumbnailQuery(
    Guid PhotoId,
    string UserId
) : IQuery<Stream>;
