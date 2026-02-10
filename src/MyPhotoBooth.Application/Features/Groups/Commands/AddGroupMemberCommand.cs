using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Groups.Commands;

public record AddGroupMemberCommand(
    Guid GroupId,
    string MemberEmail,
    string UserId
) : ICommand<GroupMemberResponse>;
