using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Infrastructure.Persistence;

namespace MyPhotoBooth.Infrastructure.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        AppDbContext dbContext,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only use transactions for commands (writes)
        if (!typeof(TRequest).Name.EndsWith("Command"))
        {
            return await next();
        }

        IDbContextTransaction? transaction = null;

        try
        {
            transaction = await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

            _logger.LogDebug(
                "Started transaction for {RequestType}",
                typeof(TRequest).Name);

            var response = await next();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogDebug(
                "Committed transaction for {RequestType}",
                typeof(TRequest).Name);

            return response;
        }
        catch (Exception)
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning(
                    "Rolled back transaction for {RequestType}",
                    typeof(TRequest).Name);
            }
            throw;
        }
        finally
        {
            if (transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }
    }
}
