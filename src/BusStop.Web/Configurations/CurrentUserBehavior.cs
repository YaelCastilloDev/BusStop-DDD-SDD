using Ardalis.Result;
using Ardalis.SharedKernel;
using Ardalis.Specification;
using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.Web.Configurations;

// DESIGN NOTE: Internal User.Id resolution via JWT claim
// ─────────────────────────────────────────────────────
// The ideal path is reading a custom `user_id` claim directly from the JWT,
// avoiding a DB lookup entirely. Keycloak can map a `_busstop_id` user attribute
// to a `user_id` claim via a protocol mapper.
//
// We intentionally do NOT automate writing this attribute from the backend.
// Design alternatives considered and deferred:
//   a) RegisterUserHandler calling KeycloakAdminService — rejected: mixes
//      infrastructure concern into UseCases layer.
//   b) UserRegisteredIntegrationHandler (domain event handler) extending to set
//      the attribute — viable but adds complexity; deferred to a future iteration.
//
// For now, the claim may or may not be present. This behavior handles both paths:
//   1. Claim present → zero-cost resolution (no DB call).
//   2. Claim missing → fallback to existing GetUserByExternalIdAsync DB lookup.
// Once Keycloak is configured with the mapper and user attributes are backfilled,
// the fallback path becomes dead code for the normal case.
public sealed class CurrentUserBehavior<TRequest, TResponse>(
    IHttpContextAccessor httpContextAccessor,
    ScopedCurrentUser currentUser,
    IReadRepository<User> userReadRepository)
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

            var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst("user_id")?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && long.TryParse(userIdClaim, out var userId))
            {
                currentUser.Id = userId;
            }
            else
            {
                var spec = new UserByExternalIdSpec(sub);
                var user = await userReadRepository.FirstOrDefaultAsync(spec, cancellationToken);
                if (user is not null)
                    currentUser.Id = user.Id;
            }
        }

        return await next(request, cancellationToken);
    }
}
