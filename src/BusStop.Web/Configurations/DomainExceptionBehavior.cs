using BusStop.Core.Exceptions;
using Mediator;

namespace BusStop.Web.Configurations;

public sealed class DomainExceptionBehavior<TRequest, TResponse>(
    ILogger<DomainExceptionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    // NOTE: This is a safety net for the future. The primary error strategy is
    // Result<T> from static factory methods, not throwing exceptions. This behavior
    // catches any DomainValidationException that might slip through from legacy or
    // edge-case code paths.
{
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(request, cancellationToken);
        }
        catch (DomainValidationException ex)
        {
            logger.LogWarning(ex, "Domain validation failed in {RequestType}", typeof(TRequest).Name);

            if (typeof(TResponse).IsGenericType
                && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var error = typeof(TResponse)
                    .GetMethod("Error", [typeof(string)])
                    ?.Invoke(null, [ex.Message]);
                return (TResponse)error!;
            }

            if (typeof(TResponse) == typeof(Result))
                return (TResponse)(object)Result.Error(ex.Message);

            throw;
        }
    }
}
