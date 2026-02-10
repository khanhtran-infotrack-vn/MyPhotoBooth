using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Tags.Queries;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Tags.Handlers;

public class GetTagPhotosQueryHandler : IRequestHandler<GetTagPhotosQuery, Result<List<PhotoListResponse>>>
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<GetTagPhotosQueryHandler> _logger;

    public GetTagPhotosQueryHandler(
        ITagRepository tagRepository,
        ILogger<GetTagPhotosQueryHandler> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    public async Task<Result<List<PhotoListResponse>>> Handle(
        GetTagPhotosQuery request,
        CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(request.TagId, cancellationToken);
        if (tag == null)
            return Result.Failure<List<PhotoListResponse>>(Errors.Tags.NotFound);
        if (tag.UserId != request.UserId)
            return Result.Failure<List<PhotoListResponse>>(Errors.General.Unauthorized);

        var photos = tag.PhotoTags.Select(pt => new PhotoListResponse
        {
            Id = pt.Photo.Id,
            OriginalFileName = pt.Photo.OriginalFileName,
            Width = pt.Photo.Width,
            Height = pt.Photo.Height,
            CapturedAt = pt.Photo.CapturedAt,
            UploadedAt = pt.Photo.UploadedAt,
            ThumbnailPath = pt.Photo.ThumbnailPath
        }).ToList();

        return Result.Success(photos);
    }
}
