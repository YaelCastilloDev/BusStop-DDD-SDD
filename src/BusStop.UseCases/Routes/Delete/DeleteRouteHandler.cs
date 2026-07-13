using BusStop.Core.Interfaces;
using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Specifications;
using BusStop.Core.UserAggregate;

namespace BusStop.UseCases.Routes.Delete;

public sealed class DeleteRouteHandler(
  IRepository<Route> repository,
  ICurrentUser currentUser) : ICommandHandler<DeleteRouteCommand, Result>
{
  public async ValueTask<Result> Handle(DeleteRouteCommand request, CancellationToken cancellationToken)
  {
    if (currentUser.Id <= 0)
      return Result.NotFound("User not found.");

    var routeResult = await repository.FindRequiredAsync(new RouteByIdSpec(new RouteId(request.RouteId)), "Route not found.", cancellationToken);
    if (!routeResult.IsSuccess)
      return Result.NotFound("Route not found.");
    var route = routeResult.Value;

    var deleteResult = route.Delete(new UserId(currentUser.Id));
    if (!deleteResult.IsSuccess)
      return Result.Error(new ErrorList(deleteResult.Errors));

    await repository.UpdateAsync(route, cancellationToken);

    return Result.Success();
  }
}
