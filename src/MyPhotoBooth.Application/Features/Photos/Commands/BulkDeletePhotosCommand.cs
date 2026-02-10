using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Commands;

public record BulkDeletePhotosCommand(
    List<Guid> PhotoIds,
    string UserId
) : ICommand<BulkOperationResultDto>;
