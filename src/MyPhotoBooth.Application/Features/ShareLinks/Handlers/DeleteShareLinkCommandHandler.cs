using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Features.ShareLinks.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.ShareLinks.Handlers;

public class DeleteShareLinkCommandHandler : IRequestHandler<DeleteShareLinkCommand, Result>
{
    private readonly IShareLinkRepository _shareLinkRepository;
    private readonly ILogger<DeleteShareLinkCommandHandler> _logger;

    public DeleteShareLinkCommandHandler(
        IShareLinkRepository shareLinkRepository,
        ILogger<DeleteShareLinkCommandHandler> logger)
    {
        _shareLinkRepository = shareLinkRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteShareLinkCommand request,
        CancellationToken cancellationToken)
    {
        var shareLink = await _shareLinkRepository.GetByIdAsync(request.ShareLinkId, cancellationToken);

        if (shareLink == null)
            return Result.Failure(Errors.ShareLinks.NotFound);

        if (shareLink.UserId != request.UserId)
            return Result.Failure(Errors.General.Unauthorized);

        shareLink.RevokedAt = DateTime.UtcNow;
        await _shareLinkRepository.UpdateAsync(shareLink, cancellationToken);

        _logger.LogInformation("Share link revoked: {ShareLinkId}", request.ShareLinkId);
        return Result.Success();
    }
}
