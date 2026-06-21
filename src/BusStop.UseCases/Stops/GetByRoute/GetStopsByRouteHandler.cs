using BusStop.Core.StopAggregate;
using BusStop.Core.StopAggregate.Specifications;
using BusStop.Core.RouteAggregate;

namespace BusStop.UseCases.Stops.GetByRoute;

public sealed class GetStopsByRouteHandler(IReadRepository<Stop> repository) : IQueryHandler<GetStopsByRouteQuery, Result<List<StopResponse>>>
{
  public async ValueTask<Result<List<StopResponse>>> Handle(GetStopsByRouteQuery request, CancellationToken cancellationToken)
  {
    var spec = new StopsByRouteSpec(new RouteId(request.RouteId));
    var stops = await repository.ListAsync(spec, cancellationToken);

    var responses = stops
      .Where(s => !s.IsDeleted)
      .Select(s => new StopResponse(s.Id, s.Name.Value, s.Location.Latitude, s.Location.Longitude, s.RouteId.Value, s.IsDeleted))
      .ToList();

    return responses;
  }
}

