using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.ShareLinks.Queries;

public record GetShareLinksQuery(
    string UserId,
    string BaseUrl
) : IQuery<List<ShareLinkResponse>>;
