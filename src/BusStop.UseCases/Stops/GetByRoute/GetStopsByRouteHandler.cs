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
    var route = await routeRepository.FirstOrDefaultAsync(new RouteByIdSpec(new RouteId(request.RouteId)), cancellationToken);
    if (route is null)
      return Result<List<StopResponse>>.NotFound("Route not found.");

    var spec = new StopsByRouteSpec(new RouteId(request.RouteId));
    var stops = await repository.ListAsync(spec, cancellationToken);

    var responses = stops
      .Where(s => !s.IsDeleted)
      .Select(s => new StopResponse(s.Id, s.Name.Value, s.Location.Latitude, s.Location.Longitude, s.RouteId.Value, s.IsDeleted))
      .ToList();

    return responses;
  }
}

