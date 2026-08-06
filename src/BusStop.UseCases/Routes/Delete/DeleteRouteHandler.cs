using BusStop.Core.Interfaces;
using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Events;
using BusStop.Core.RouteAggregate.Specifications;
using BusStop.Core.UserAggregate;

namespace BusStop.UseCases.Routes.Delete;

public sealed class DeleteRouteHandler(
  IRepository<Route> repository,
  ICurrentUser currentUser,
  IPublisher publisher) : ICommandHandler<DeleteRouteCommand, Result>
{
  public async ValueTask<Result> Handle(DeleteRouteCommand request, CancellationToken cancellationToken)
  {
    if (currentUser.Id <= 0)
      return Result.NotFound("User not found.");

    var routeResult = await repository.FindRequiredAsync(new RouteByIdSpec(new RouteId(request.RouteId)), "Route not found.", cancellationToken);
    if (!routeResult.IsSuccess)
      return Result.NotFound("Route not found.");
    var route = routeResult.Value;

    await publisher.Publish(new RouteDeletedEvent(route.Id, currentUser.Id), cancellationToken);

    await repository.DeleteAsync(route, cancellationToken);

    return Result.Success();
  }
}
