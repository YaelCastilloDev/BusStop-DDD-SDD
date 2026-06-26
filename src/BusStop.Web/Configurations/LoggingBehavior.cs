using System.Diagnostics;
using Ardalis.GuardClauses;
using Mediator;

namespace BusStop.Web.Configurations;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
        }

        var sw = Stopwatch.StartNew();

        var response = await next(request, cancellationToken);

        sw.Stop();

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Handled {RequestName} with {Response} in {ElapsedMilliseconds} ms",
                typeof(TRequest).Name, response, sw.ElapsedMilliseconds);
        }

        return response;
    }
}
