using BusStop.Core.Interfaces;
using BusStop.Core.RouteAggregate;

namespace BusStop.UseCases.Routes.Create;

public sealed class CreateRouteHandler(
  IRepository<Route> repository,
  ICurrentUser currentUser) : ICommandHandler<CreateRouteCommand, Result<RouteResponse>>
{
  public async ValueTask<Result<RouteResponse>> Handle(CreateRouteCommand request, CancellationToken cancellationToken)
  {
    if (currentUser.Id <= 0)
      return Result<RouteResponse>.NotFound("User not found.");

    return await repository.CreateAsync(Route.Create(request.Name, currentUser.Id), r => r.ToResponse(), cancellationToken);
  }
}
