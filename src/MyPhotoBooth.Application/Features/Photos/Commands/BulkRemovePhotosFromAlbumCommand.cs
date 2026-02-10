using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Commands;

public record BulkRemovePhotosFromAlbumCommand(
    List<Guid> PhotoIds,
    Guid AlbumId,
    string UserId
) : ICommand<BulkOperationResultDto>;
