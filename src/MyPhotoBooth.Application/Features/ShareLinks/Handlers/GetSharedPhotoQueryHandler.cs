using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Features.ShareLinks.Queries;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.ShareLinks.Handlers;

public class GetSharedPhotoQueryHandler : IRequestHandler<GetSharedPhotoQuery, Result<Photo>>
{
    private readonly IShareLinkRepository _shareLinkRepository;
    private readonly ILogger<GetSharedPhotoQueryHandler> _logger;

    public GetSharedPhotoQueryHandler(
        IShareLinkRepository shareLinkRepository,
        ILogger<GetSharedPhotoQueryHandler> logger)
    {
        _shareLinkRepository = shareLinkRepository;
        _logger = logger;
    }

    public async Task<Result<Photo>> Handle(
        GetSharedPhotoQuery request,
        CancellationToken cancellationToken)
    {
        var shareLink = await _shareLinkRepository.GetByTokenAsync(request.Token, cancellationToken);

        if (shareLink == null)
            return Result.Failure<Photo>(Errors.ShareLinks.NotFound);

        if (!shareLink.IsActive)
            return Result.Failure<Photo>(Errors.ShareLinks.Revoked);

        if (shareLink.IsExpired)
            return Result.Failure<Photo>(Errors.ShareLinks.Expired);

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
            return Result.Failure<Photo>(Errors.Photos.NotFound);

        return Result.Success(photo);
    }
}
