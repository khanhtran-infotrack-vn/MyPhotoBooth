using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Commands;

public record BulkToggleFavoritePhotosCommand(
    List<Guid> PhotoIds,
    string UserId,
    bool Favorite
) : ICommand<BulkOperationResultDto>;
