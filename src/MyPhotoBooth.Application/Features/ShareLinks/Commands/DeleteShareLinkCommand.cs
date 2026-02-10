using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.ShareLinks.Commands;

public record DeleteShareLinkCommand(
    Guid ShareLinkId,
    string UserId
) : ICommand;
