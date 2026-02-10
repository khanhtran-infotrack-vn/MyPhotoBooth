using MediatR;
using MyPhotoBooth.Application.Common.Requests;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.ShareLinks.Queries;

public record GetShareLinkByTokenQuery(
    string Token
) : IQuery<ShareLink>;

public record GetSharedPhotoQuery(
    string Token,
    Guid PhotoId
) : IQuery<Photo>;

public record GetSharedFileQuery(
    string Token,
    Guid PhotoId
) : IQuery<(Photo Photo, bool AllowDownload)>;
