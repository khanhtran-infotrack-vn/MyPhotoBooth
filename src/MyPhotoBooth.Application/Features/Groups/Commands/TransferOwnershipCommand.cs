using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Groups.Commands;

public record TransferOwnershipCommand(
    Guid GroupId,
    string NewOwnerId,
    string UserId
) : ICommand<GroupMemberResponse>;
