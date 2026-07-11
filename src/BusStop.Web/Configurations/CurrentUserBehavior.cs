using Ardalis.Result;
using BusStop.Core.Interfaces;

namespace BusStop.Web.Configurations;

public sealed class CurrentUserBehavior<TRequest, TResponse>(IHttpContextAccessor httpContextAccessor)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IRequireAuthenticatedUser userRequest)
        {
            var sub = httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(sub))
                return (TResponse)(object)Result.Unauthorized("Authentication required.");
            userRequest.Sub = sub;
        }

        return await next(request, cancellationToken);
    }
}
