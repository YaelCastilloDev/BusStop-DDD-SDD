using BusStop.Core.Interfaces;
using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Specifications;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.UseCases.Routes.Delete;

public sealed class DeleteRouteHandler(
  IRepository<Route> repository,
  IReadRepository<User> userRepository) : ICommandHandler<DeleteRouteCommand, Result>
{
  public async ValueTask<Result> Handle(DeleteRouteCommand request, CancellationToken cancellationToken)
  {
    if (request.RouteId <= 0)
      return Result.Error("Route ID is required.");
    if (string.IsNullOrEmpty(request.Sub))
      return Result.Unauthorized("Authentication required.");

    var user = await userRepository.FirstOrDefaultAsync(new UserByExternalIdSpec(request.Sub), cancellationToken);
    if (user is null)
      return Result.NotFound("User not found. Please register first.");

    var spec = new RouteByIdSpec(new RouteId(request.RouteId));
    var route = await repository.FirstOrDefaultAsync(spec, cancellationToken);

    if (route is null)
      return Result.NotFound("Route not found.");

    if (route.IsDeleted)
      return Result.Error("Route is already deleted.");

    route.Delete(new UserId(user.Id));

    await repository.UpdateAsync(route, cancellationToken);

    return Result.Success();
  }
}

