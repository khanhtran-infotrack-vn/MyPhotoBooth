using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Features.ShareLinks.Queries;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.ShareLinks.Handlers;

public class GetSharedFileQueryHandler : IRequestHandler<GetSharedFileQuery, Result<(Photo Photo, bool AllowDownload)>>
{
    private readonly IShareLinkRepository _shareLinkRepository;
    private readonly ILogger<GetSharedFileQueryHandler> _logger;

    public GetSharedFileQueryHandler(
        IShareLinkRepository shareLinkRepository,
        ILogger<GetSharedFileQueryHandler> logger)
    {
        _shareLinkRepository = shareLinkRepository;
        _logger = logger;
    }

    public async Task<Result<(Photo Photo, bool AllowDownload)>> Handle(
        GetSharedFileQuery request,
        CancellationToken cancellationToken)
    {
        var shareLink = await _shareLinkRepository.GetByTokenAsync(request.Token, cancellationToken);

        if (shareLink == null)
            return Result.Failure<(Photo, bool)>(Errors.ShareLinks.NotFound);

        if (!shareLink.IsActive)
            return Result.Failure<(Photo, bool)>(Errors.ShareLinks.Revoked);

        if (shareLink.IsExpired)
            return Result.Failure<(Photo, bool)>(Errors.ShareLinks.Expired);

        // Check download permission
        if (!shareLink.AllowDownload)
            return Result.Failure<(Photo, bool)>(Errors.ShareLinks.DownloadNotAllowed);

        // Find the photo in the share link
        Photo? photo = null;
        if (shareLink.Type == ShareLinkType.Photo && shareLink.PhotoId == request.PhotoId)
        {
            photo = shareLink.Photo;
        }
        else if (shareLink.Type == ShareLinkType.Album && shareLink.Album != null)
        {
            photo = shareLink.Album.AlbumPhotos
                .Select(ap => ap.Photo)
                .FirstOrDefault(p => p.Id == request.PhotoId);
        }

        if (photo == null)
            return Result.Failure<(Photo, bool)>(Errors.Photos.NotFound);

        return Result.Success((photo, shareLink.AllowDownload));
    }
}
