using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Queries;

public record BulkDownloadPhotosQuery(
    List<Guid> PhotoIds,
    string UserId
) : IQuery<BulkDownloadResultDto>;
