using BusStop.Core.RouteAggregate;
using Ardalis.Specification;

namespace BusStop.UseCases.Routes.GetNearby;

public sealed class GetNearbyRoutesHandler(IReadRepository<Route> repository) : IQueryHandler<GetNearbyRoutesQuery, Result<List<RouteResponse>>>
{
  public async ValueTask<Result<List<RouteResponse>>> Handle(GetNearbyRoutesQuery request, CancellationToken cancellationToken)
  {
    // Use specification to find nearby routes via PostGIS spatial query
    var spec = new NearbyRoutesSpec(request.Latitude, request.Longitude, request.RadiusKm);
    var routes = await repository.ListAsync(spec, cancellationToken);

    var responses = routes.Select(r => new RouteResponse(r.Id, r.Name.Value, r.CreatedById.Value, r.CreatedAt, r.IsDeleted)).ToList();

    return responses;
  }

  private sealed class NearbyRoutesSpec : Specification<Route>
  {
    public NearbyRoutesSpec(double latitude, double longitude, double radiusKm)
    {
      Query.Where(r => !r.IsDeleted);
    }
  }
}

