using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Features.Tags.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Tags.Handlers;

public class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand, Result>
{
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<DeleteTagCommandHandler> _logger;

    public DeleteTagCommandHandler(
        ITagRepository tagRepository,
        ILogger<DeleteTagCommandHandler> logger)
    {
        _tagRepository = tagRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteTagCommand request,
        CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(request.TagId, cancellationToken);

        if (tag == null)
            return Result.Failure(Errors.Tags.NotFound);

        if (tag.UserId != request.UserId)
            return Result.Failure(Errors.General.Unauthorized);

        await _tagRepository.DeleteAsync(request.TagId, cancellationToken);

        _logger.LogInformation("Tag deleted: {TagId}", request.TagId);
        return Result.Success();
    }
}
