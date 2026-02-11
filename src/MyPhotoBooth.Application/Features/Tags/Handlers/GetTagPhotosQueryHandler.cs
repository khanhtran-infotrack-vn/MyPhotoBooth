using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Pagination;
using MyPhotoBooth.Application.Features.Tags.Queries;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using CSharpFunctionalExtensions;

namespace MyPhotoBooth.Application.Features.Tags.Handlers;

public class GetTagPhotosQueryHandler : IRequestHandler<GetTagPhotosQuery, Result<PaginatedResult<PhotoListResponse>>>
{
    private readonly ITagRepository _tagRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly ILogger<GetTagPhotosQueryHandler> _logger;

    public GetTagPhotosQueryHandler(
        ITagRepository tagRepository,
        IPhotoRepository photoRepository,
        ILogger<GetTagPhotosQueryHandler> logger)
    {
        _tagRepository = tagRepository;
        _photoRepository = photoRepository;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<PhotoListResponse>>> Handle(
        GetTagPhotosQuery request,
        CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(request.TagId, cancellationToken);
        if (tag == null)
            return Result.Failure<PaginatedResult<PhotoListResponse>>(Errors.Tags.NotFound);
        if (tag.UserId != request.UserId)
            return Result.Failure<PaginatedResult<PhotoListResponse>>(Errors.General.Unauthorized);

        var allPhotos = tag.PhotoTags
            .Select(pt => pt.Photo)
            .OrderByDescending(p => p.UploadedAt)
            .ToList();

        var totalCount = allPhotos.Count;
        var skip = (request.Page - 1) * request.PageSize;
        var pagedPhotosEntities = allPhotos
            .Skip(skip)
            .Take(request.PageSize)
            .ToList();

        // Get favorite status for all photos in batch
        var photoIds = pagedPhotosEntities.Select(p => p.Id).ToList();
        var favoriteStatus = photoIds.Any()
            ? await _photoRepository.GetFavoriteStatusAsync(photoIds, request.UserId, cancellationToken)
            : new Dictionary<Guid, bool>();

        var pagedPhotos = pagedPhotosEntities.Select(p => new PhotoListResponse
        {
            Id = p.Id,
            OriginalFileName = p.OriginalFileName,
            Width = p.Width,
            Height = p.Height,
            CapturedAt = p.CapturedAt,
            UploadedAt = p.UploadedAt,
            ThumbnailPath = p.ThumbnailPath,
            IsFavorite = favoriteStatus.GetValueOrDefault(p.Id, false)
        }).ToList();

        _logger.LogInformation(
            "Retrieved {Count} photos for tag {TagId}, page {Page}",
            pagedPhotos.Count,
            request.TagId,
            request.Page);

        return Result.Success(PaginatedResult<PhotoListResponse>.Create(
            pagedPhotos,
            request.Page,
            request.PageSize,
            totalCount));
    }
}
