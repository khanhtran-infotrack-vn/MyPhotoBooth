using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Features.ShareLinks.Queries;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.ShareLinks.Handlers;

public class GetShareLinkByTokenQueryHandler : IRequestHandler<GetShareLinkByTokenQuery, Result<ShareLink>>
{
    private readonly IShareLinkRepository _shareLinkRepository;
    private readonly ILogger<GetShareLinkByTokenQueryHandler> _logger;

    public GetShareLinkByTokenQueryHandler(
        IShareLinkRepository shareLinkRepository,
        ILogger<GetShareLinkByTokenQueryHandler> logger)
    {
        _shareLinkRepository = shareLinkRepository;
        _logger = logger;
    }

    public async Task<Result<ShareLink>> Handle(
        GetShareLinkByTokenQuery request,
        CancellationToken cancellationToken)
    {
        var shareLink = await _shareLinkRepository.GetByTokenAsync(request.Token, cancellationToken);

        if (shareLink == null)
            return Result.Failure<ShareLink>(Errors.ShareLinks.NotFound);

        return Result.Success(shareLink);
    }
}
