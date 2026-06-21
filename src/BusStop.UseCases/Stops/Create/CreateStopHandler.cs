using BusStop.Core.StopAggregate;

namespace BusStop.UseCases.Stops.Create;

public sealed class CreateStopHandler(IRepository<Stop> repository) : ICommandHandler<CreateStopCommand, Result<StopResponse>>
{
  public async ValueTask<Result<StopResponse>> Handle(CreateStopCommand request, CancellationToken cancellationToken)
  {
    var result = Stop.Create(request.Name, request.Latitude, request.Longitude, request.RouteId);
    if (!result.IsSuccess)
      return Result<StopResponse>.Error(result.Errors.FirstOrDefault());

    var stop = result.Value;
    var created = await repository.AddAsync(stop, cancellationToken);

    return new StopResponse(created.Id, created.Name.Value, created.Location.Latitude, created.Location.Longitude, created.RouteId.Value, created.IsDeleted);
  }
}

