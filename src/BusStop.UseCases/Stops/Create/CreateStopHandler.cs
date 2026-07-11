using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Specifications;
using BusStop.Core.StopAggregate;

namespace BusStop.UseCases.Stops.Create;

public sealed class CreateStopHandler(
  IRepository<Stop> repository,
  IReadRepository<Route> routeRepository) : ICommandHandler<CreateStopCommand, Result<StopResponse>>
{
  public async ValueTask<Result<StopResponse>> Handle(CreateStopCommand request, CancellationToken cancellationToken)
  {
    var routeResult = await routeRepository.FindRequiredAsync(new RouteByIdSpec(new RouteId(request.RouteId)), "Route not found.", cancellationToken);
    if (!routeResult.IsSuccess)
      return Result<StopResponse>.NotFound("Route not found.");
    var route = routeResult.Value;

    return await repository.CreateAsync(Stop.Create(request.Name, request.Latitude, request.Longitude, request.RouteId), s => s.ToResponse(), cancellationToken);
  }
}
