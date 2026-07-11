using BusStop.Core.Interfaces;
using BusStop.Core.RouteAggregate;
using BusStop.Core.UserAggregate;
using BusStop.UseCases.Users;

namespace BusStop.UseCases.Routes.Create;

public sealed class CreateRouteHandler(
  IRepository<Route> repository,
  IReadRepository<User> userRepository) : ICommandHandler<CreateRouteCommand, Result<RouteResponse>>
{
  public async ValueTask<Result<RouteResponse>> Handle(CreateRouteCommand request, CancellationToken cancellationToken)
  {
    var userResult = await userRepository.GetUserByExternalIdAsync(request.Sub, cancellationToken);
    if (!userResult.IsSuccess)
      return Result<RouteResponse>.NotFound("User not found.");
    var user = userResult.Value;

    return await repository.CreateAsync(Route.Create(request.Name, user.Id), r => r.ToResponse(), cancellationToken);
  }
}
