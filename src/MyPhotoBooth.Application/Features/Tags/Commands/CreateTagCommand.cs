using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Tags.Commands;

public record CreateTagCommand(
    string Name,
    string UserId
) : ICommand<TagResponse>;
