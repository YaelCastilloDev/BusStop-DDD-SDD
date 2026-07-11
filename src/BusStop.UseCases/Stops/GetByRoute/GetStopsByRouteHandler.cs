using BusStop.Core.StopAggregate;
using BusStop.Core.StopAggregate.Specifications;
using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Specifications;

namespace BusStop.UseCases.Stops.GetByRoute;

public sealed class GetStopsByRouteHandler(
  IReadRepository<Stop> repository,
  IReadRepository<Route> routeRepository) : IQueryHandler<GetStopsByRouteQuery, Result<List<StopResponse>>>
{
  public async ValueTask<Result<List<StopResponse>>> Handle(GetStopsByRouteQuery request, CancellationToken cancellationToken)
  {
    var routeResult = await routeRepository.FindRequiredAsync(new RouteByIdSpec(new RouteId(request.RouteId)), "Route not found.", cancellationToken);
    if (!routeResult.IsSuccess)
      return Result<List<StopResponse>>.NotFound("Route not found.");
    var route = routeResult.Value;

    var spec = new StopsByRouteSpec(new RouteId(request.RouteId));
    var stops = await repository.ListAsync(spec, cancellationToken);

    var responses = stops
      .Where(s => !s.IsDeleted)
      .Select(s => s.ToResponse())
      .ToList();

    return responses;
  }
}

