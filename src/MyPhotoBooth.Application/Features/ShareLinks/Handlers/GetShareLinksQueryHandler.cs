using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.ShareLinks.Queries;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.ShareLinks.Handlers;

public class GetShareLinksQueryHandler : IRequestHandler<GetShareLinksQuery, Result<List<ShareLinkResponse>>>
{
    private readonly IShareLinkRepository _shareLinkRepository;
    private readonly ILogger<GetShareLinksQueryHandler> _logger;

    public GetShareLinksQueryHandler(
        IShareLinkRepository shareLinkRepository,
        ILogger<GetShareLinksQueryHandler> logger)
    {
        _shareLinkRepository = shareLinkRepository;
        _logger = logger;
    }

    public async Task<Result<List<ShareLinkResponse>>> Handle(
        GetShareLinksQuery request,
        CancellationToken cancellationToken)
    {
        var shareLinks = await _shareLinkRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        var response = shareLinks.Select(sl => new ShareLinkResponse
        {
            Id = sl.Id,
            Token = sl.Token,
            Type = sl.Type,
            PhotoId = sl.PhotoId,
            AlbumId = sl.AlbumId,
            TargetName = sl.Type == ShareLinkType.Photo
                ? sl.Photo?.OriginalFileName
                : sl.Album?.Name,
            HasPassword = sl.PasswordHash != null,
            ExpiresAt = sl.ExpiresAt,
            AllowDownload = sl.AllowDownload,
            ShareUrl = $"{request.BaseUrl}/shared/{sl.Token}",
            IsActive = sl.IsActive,
            CreatedAt = sl.CreatedAt
        }).ToList();

        return Result.Success(response);
    }
}
