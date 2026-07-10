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
    var route = await routeRepository.FirstOrDefaultAsync(new RouteByIdSpec(new RouteId(request.RouteId)), cancellationToken);
    if (route is null)
      return Result<StopResponse>.NotFound("Route not found.");

    var stopResult = Stop.Create(request.Name, request.Latitude, request.Longitude, request.RouteId);
    if (!stopResult.IsSuccess)
      return Result<StopResponse>.Error(new ErrorList(stopResult.Errors));

    var stop = stopResult.Value;
    var created = await repository.AddAsync(stop, cancellationToken);

    return new StopResponse(created.Id, created.Name.Value, created.Location.Latitude, created.Location.Longitude, created.RouteId.Value, created.IsDeleted);
  }
}
