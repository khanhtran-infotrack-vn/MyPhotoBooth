using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.ShareLinks.Commands;

public record CreateShareLinkCommand(
    ShareLinkType Type,
    Guid? PhotoId,
    Guid? AlbumId,
    DateTime? ExpiresAt,
    bool AllowDownload,
    string? Password,
    string UserId,
    string BaseUrl
) : ICommand<ShareLinkResponse>;
