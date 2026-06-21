using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Specifications;

namespace BusStop.UseCases.Routes.GetById;

public sealed class GetRouteByIdHandler(IReadRepository<Route> repository) : IQueryHandler<GetRouteByIdQuery, Result<RouteResponse>>
{
  public async ValueTask<Result<RouteResponse>> Handle(GetRouteByIdQuery request, CancellationToken cancellationToken)
  {
    var spec = new RouteByIdSpec(new RouteId(request.RouteId));
    var route = await repository.FirstOrDefaultAsync(spec, cancellationToken);

    if (route is null)
      return Result<RouteResponse>.NotFound("Route not found.");

    return new RouteResponse(route.Id, route.Name.Value, route.CreatedById.Value, route.CreatedAt, route.IsDeleted);
  }
}

