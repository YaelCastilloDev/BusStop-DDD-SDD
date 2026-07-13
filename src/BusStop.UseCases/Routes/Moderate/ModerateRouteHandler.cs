using BusStop.Core.Interfaces;
using BusStop.Core.ModerationActionAggregate;
using BusStop.Core.ModerationActionAggregate.Events;
using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Specifications;
using BusStop.Core.UserAggregate;

namespace BusStop.UseCases.Routes.Moderate;

public sealed class ModerateRouteHandler(
  IRepository<Route> routeRepository,
  IRepository<ModerationAction> moderationActionRepository,
  ICurrentUser currentUser,
  IPublisher publisher) : ICommandHandler<ModerateRouteCommand, Result>
{
  public async ValueTask<Result> Handle(ModerateRouteCommand request, CancellationToken cancellationToken)
  {
    if (currentUser.Id <= 0)
      return Result.NotFound("User not found.");

    var routeResult = await routeRepository.FindRequiredAsync(new RouteByIdSpec(new RouteId(request.RouteId)), "Route not found.", cancellationToken);
    if (!routeResult.IsSuccess)
      return Result.NotFound("Route not found.");
    var route = routeResult.Value;

    var moderateResult = route.Moderate(new UserId(currentUser.Id));
    if (!moderateResult.IsSuccess)
      return Result.Error(new ErrorList(moderateResult.Errors));

    var actionResult = ModerationAction.Create(TargetType.Route, route.Id, route.CreatedById.Value, currentUser.Id, request.Category, request.Reason);
    if (!actionResult.IsSuccess)
      return Result.Error(new ErrorList(actionResult.Errors));

    var moderationAction = actionResult.Value;
    await moderationActionRepository.AddAsync(moderationAction, cancellationToken);

    await publisher.Publish(new ModerationActionRecordedEvent(
        moderationAction.Id,
        moderationAction.TargetType,
        moderationAction.TargetId,
        moderationAction.UserId.Value,
        moderationAction.IssuedBy.Value,
        moderationAction.Category,
        moderationAction.Reason.Value,
        moderationAction.IssuedAt), cancellationToken);

    await routeRepository.UpdateAsync(route, cancellationToken);

    return Result.Success();
  }
}
