using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.ShareLinks.Queries;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.ShareLinks.Handlers;

public class ValidateShareLinkQueryHandler : IRequestHandler<ValidateShareLinkQuery, Result<SharedContentResponse>>
{
    private readonly IShareLinkRepository _shareLinkRepository;
    private readonly IPasswordHasher<object> _passwordHasher;
    private readonly ILogger<ValidateShareLinkQueryHandler> _logger;

    public ValidateShareLinkQueryHandler(
        IShareLinkRepository shareLinkRepository,
        ILogger<ValidateShareLinkQueryHandler> logger)
    {
        _shareLinkRepository = shareLinkRepository;
        _passwordHasher = new PasswordHasher<object>();
        _logger = logger;
    }

    public async Task<Result<SharedContentResponse>> Handle(
        ValidateShareLinkQuery request,
        CancellationToken cancellationToken)
    {
        var shareLink = await _shareLinkRepository.GetByTokenAsync(request.Token, cancellationToken);

        if (shareLink == null)
            return Result.Failure<SharedContentResponse>(Errors.ShareLinks.NotFound);

        if (shareLink.IsExpired)
            return Result.Failure<SharedContentResponse>(Errors.ShareLinks.Expired);

        if (!shareLink.IsActive)
            return Result.Failure<SharedContentResponse>(Errors.ShareLinks.Revoked);

        // Verify password if required
        if (shareLink.PasswordHash != null)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                return Result.Failure<SharedContentResponse>(Errors.ShareLinks.PasswordRequired);

            var result = _passwordHasher.VerifyHashedPassword(new object(), shareLink.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                return Result.Failure<SharedContentResponse>(Errors.ShareLinks.InvalidPassword);
        }

        // Build response based on type
        if (shareLink.Type == ShareLinkType.Photo && shareLink.Photo != null)
        {
            return Result.Success(new SharedContentResponse
            {
                Type = ShareLinkType.Photo,
                Photo = new SharedPhotoResponse
                {
                    Id = shareLink.Photo.Id,
                    FileName = shareLink.Photo.OriginalFileName,
                    Width = shareLink.Photo.Width,
                    Height = shareLink.Photo.Height,
                    CapturedAt = shareLink.Photo.CapturedAt,
                    UploadedAt = shareLink.Photo.UploadedAt,
                    Description = shareLink.Photo.Description,
                    AllowDownload = shareLink.AllowDownload
                }
            });
        }
        else if (shareLink.Type == ShareLinkType.Album && shareLink.Album != null)
        {
            var photos = shareLink.Album.AlbumPhotos
                .OrderBy(ap => ap.SortOrder)
                .Select(ap => new SharedPhotoResponse
                {
                    Id = ap.Photo.Id,
                    FileName = ap.Photo.OriginalFileName,
                    Width = ap.Photo.Width,
                    Height = ap.Photo.Height,
                    CapturedAt = ap.Photo.CapturedAt,
                    UploadedAt = ap.Photo.UploadedAt,
                    Description = ap.Photo.Description,
                    AllowDownload = shareLink.AllowDownload
                }).ToList();

            return Result.Success(new SharedContentResponse
            {
                Type = ShareLinkType.Album,
                Album = new SharedAlbumResponse
                {
                    Name = shareLink.Album.Name,
                    Description = shareLink.Album.Description,
                    AllowDownload = shareLink.AllowDownload,
                    Photos = photos
                }
            });
        }

        return Result.Failure<SharedContentResponse>(Errors.ShareLinks.NotFound);
    }
}
