using BusStop.Core.Interfaces;
using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Specifications;
using BusStop.Core.UserAggregate;
using BusStop.UseCases.Users;

namespace BusStop.UseCases.Routes.Delete;

public sealed class DeleteRouteHandler(
  IRepository<Route> repository,
  IReadRepository<User> userRepository) : ICommandHandler<DeleteRouteCommand, Result>
{
  public async ValueTask<Result> Handle(DeleteRouteCommand request, CancellationToken cancellationToken)
  {
    var userResult = await userRepository.GetUserByExternalIdAsync(request.Sub, cancellationToken);
    if (!userResult.IsSuccess)
      return Result.NotFound("User not found.");
    var user = userResult.Value;

    var routeResult = await repository.FindRequiredAsync(new RouteByIdSpec(new RouteId(request.RouteId)), "Route not found.", cancellationToken);
    if (!routeResult.IsSuccess)
      return Result.NotFound("Route not found.");
    var route = routeResult.Value;

    var deleteResult = route.Delete(new UserId(user.Id));
    if (!deleteResult.IsSuccess)
      return Result.Error(new ErrorList(deleteResult.Errors));

    await repository.UpdateAsync(route, cancellationToken);

    return Result.Success();
  }
}
