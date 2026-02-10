using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MyPhotoBooth.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var validationResults = await Task.WhenAll(
            _validators.Select(v =>
                v.ValidateAsync(request, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var errors = failures
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        _logger.LogWarning(
            "Validation failed for {RequestType}: {Errors}",
            typeof(TRequest).Name,
            errors);

        var errorMessage = string.Join("; ",
            failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));

        // Handle Result<T> return types
        var resultType = typeof(TResponse);
        var underlyingType = resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>)
            ? resultType.GetGenericArguments()[0]
            : null;

        if (underlyingType != null)
        {
            // Result<T>
            var failureMethod = resultType.GetMethod("Failure", new[] { typeof(string) });
            if (failureMethod != null)
            {
                return (TResponse)failureMethod.Invoke(null, new object[] { errorMessage })!;
            }
        }
        else if (resultType == typeof(Result))
        {
            // Result (non-generic) - use the generic method explicitly
            return (TResponse)(object)Result.Failure(errorMessage);
        }

        // Fallback - just continue to next handler
        return await next();
    }
}
