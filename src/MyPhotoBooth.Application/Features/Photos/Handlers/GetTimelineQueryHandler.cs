using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Pagination;
using MyPhotoBooth.Application.Features.Photos.Queries;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class GetTimelineQueryHandler : IRequestHandler<GetTimelineQuery, Result<PaginatedResult<PhotoListResponse>>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly ILogger<GetTimelineQueryHandler> _logger;

    public GetTimelineQueryHandler(
        IPhotoRepository photoRepository,
        ILogger<GetTimelineQueryHandler> logger)
    {
        _photoRepository = photoRepository;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<PhotoListResponse>>> Handle(
        GetTimelineQuery request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId ?? throw new UnauthorizedAccessException(Errors.General.Unauthorized);

        var skip = (request.Page - 1) * request.PageSize;

        DateTime? fromDate = null;
        DateTime? toDate = null;

        if (request.Year.HasValue && request.Month.HasValue)
        {
            fromDate = new DateTime(request.Year.Value, request.Month.Value, 1);
            toDate = fromDate.Value.AddMonths(1);
        }
        else if (request.Year.HasValue)
        {
            fromDate = new DateTime(request.Year.Value, 1, 1);
            toDate = fromDate.Value.AddYears(1);
        }

        var photos = await _photoRepository.GetTimelineAsync(userId, fromDate, toDate, skip, request.PageSize, cancellationToken);
        var totalCount = await _photoRepository.GetTimelineCountAsync(userId, fromDate, toDate, cancellationToken);

        var photoList = photos.Select(p => new PhotoListResponse
        {
            Id = p.Id,
            OriginalFileName = p.OriginalFileName,
            Width = p.Width,
            Height = p.Height,
            CapturedAt = p.CapturedAt,
            UploadedAt = p.UploadedAt,
            ThumbnailPath = p.ThumbnailPath
        }).ToList();

        return Result.Success(PaginatedResult<PhotoListResponse>.Create(
            photoList, request.Page, request.PageSize, totalCount));
    }
}
