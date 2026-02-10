using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Tags.Commands;

public record DeleteTagCommand(
    Guid TagId,
    string UserId
) : ICommand;
