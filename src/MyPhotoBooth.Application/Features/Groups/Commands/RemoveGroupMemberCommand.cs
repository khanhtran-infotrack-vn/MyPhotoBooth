using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Groups.Commands;

public record RemoveGroupMemberCommand(
    Guid GroupId,
    string MemberUserId,
    string UserId
) : ICommand;
