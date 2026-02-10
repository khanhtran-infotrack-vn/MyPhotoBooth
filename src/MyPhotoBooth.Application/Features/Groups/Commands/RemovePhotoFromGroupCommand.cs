using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Groups.Commands;

public record RemovePhotoFromGroupCommand(
    Guid GroupId,
    Guid PhotoId,
    string UserId
) : ICommand;
