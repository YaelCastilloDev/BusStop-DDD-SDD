using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Specifications;

namespace BusStop.UseCases.Routes.GetById;

public sealed class GetRouteByIdHandler(IReadRepository<Route> repository) : IQueryHandler<GetRouteByIdQuery, Result<RouteResponse>>
{
  public async ValueTask<Result<RouteResponse>> Handle(GetRouteByIdQuery request, CancellationToken cancellationToken)
  {
    var routeResult = await repository.FindRequiredAsync(new RouteByIdSpec(new RouteId(request.RouteId)), "Route not found.", cancellationToken);
    if (!routeResult.IsSuccess)
      return Result<RouteResponse>.NotFound("Route not found.");
    var route = routeResult.Value;

    return route.ToResponse();
  }
}

