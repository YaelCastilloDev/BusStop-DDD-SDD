using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Specifications;
using BusStop.Core.UserAggregate;

namespace BusStop.UseCases.Routes.Delete;

public sealed class DeleteRouteHandler(IRepository<Route> repository) : ICommandHandler<DeleteRouteCommand, Result>
{
  public async ValueTask<Result> Handle(DeleteRouteCommand request, CancellationToken cancellationToken)
  {
    var spec = new RouteByIdSpec(new RouteId(request.RouteId));
    var route = await repository.FirstOrDefaultAsync(spec, cancellationToken);

    if (route is null)
      return Result.NotFound("Route not found.");

    if (route.IsDeleted)
      return Result.Error("Route is already deleted.");

    route.Delete(new UserId(request.DeletedById));

    await repository.UpdateAsync(route, cancellationToken);

    return Result.Success();
  }
}

