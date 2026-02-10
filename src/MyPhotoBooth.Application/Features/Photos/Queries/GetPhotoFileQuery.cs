using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Queries;

public record GetPhotoFileQuery(
    Guid PhotoId,
    string UserId
) : IQuery<PhotoFileResult>;

public record PhotoFileResult(
    Stream Stream,
    string ContentType,
    string FileName
);
