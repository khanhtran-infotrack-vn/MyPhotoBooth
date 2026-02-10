using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Commands;

public record ToggleFavoritePhotoCommand(Guid PhotoId, string UserId) : ICommand<bool>;
