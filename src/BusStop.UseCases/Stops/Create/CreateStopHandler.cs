using BusStop.Core.Interfaces;
using BusStop.Core.RouteAggregate;
using BusStop.Core.RouteAggregate.Specifications;
using BusStop.Core.StopAggregate;

namespace BusStop.UseCases.Stops.Create;

public sealed class CreateStopHandler(
  IRepository<Stop> repository,
  IReadRepository<Route> routeRepository,
  ICurrentUser currentUser) : ICommandHandler<CreateStopCommand, Result<StopResponse>>
{
  public async ValueTask<Result<StopResponse>> Handle(CreateStopCommand request, CancellationToken cancellationToken)
  {
    if (currentUser.Id <= 0)
      return Result<StopResponse>.NotFound("User not found.");

    var routeResult = await routeRepository.FindRequiredAsync(new RouteByIdSpec(new RouteId(request.RouteId)), "Route not found.", cancellationToken);
    if (!routeResult.IsSuccess)
      return Result<StopResponse>.NotFound("Route not found.");
    var route = routeResult.Value;

    return await repository.CreateAsync(Stop.Create(request.Name, request.Latitude, request.Longitude, request.RouteId), s => s.ToResponse(), cancellationToken);
  }
}
