using BusStop.Core.Interfaces;
using Mediator;
using Polly;
using Polly.Registry;

namespace BusStop.Web.Configurations;

public sealed class ResilienceBehavior<TRequest, TResponse>(
    ResiliencePipelineProvider<string> pipelineProvider)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken ct)
    {
        var pipelineKey = request is IIdempotentRequest
            ? "idempotent-mediator"
            : "non-idempotent-mediator";

        var pipeline = pipelineProvider.GetPipeline(pipelineKey);

        var response = default(TResponse);

        var context = ResilienceContextPool.Shared.Get(ct);
        try
        {
            await pipeline.ExecuteAsync(async (ctx) =>
            {
                response = await next(request, ctx.CancellationToken);
            }, context);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }

        return response!;
    }
}
