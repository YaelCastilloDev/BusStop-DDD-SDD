using BusStop.Core.Interfaces;
using BusStop.Core.RouteAggregate;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;

namespace BusStop.UseCases.Routes.Create;

public sealed class CreateRouteHandler(
  IRepository<Route> repository,
  IReadRepository<User> userRepository) : ICommandHandler<CreateRouteCommand, Result<RouteResponse>>
{
  public async ValueTask<Result<RouteResponse>> Handle(CreateRouteCommand request, CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(request.Sub))
      return Result<RouteResponse>.Unauthorized("Authentication required.");

    var user = await userRepository.FirstOrDefaultAsync(new UserByExternalIdSpec(request.Sub), cancellationToken);
    if (user is null)
      return Result<RouteResponse>.NotFound("User not found. Please register first.");

    var routeResult = Route.Create(request.Name, user.Id);
    if (!routeResult.IsSuccess)
      return Result<RouteResponse>.Error(new ErrorList(routeResult.Errors));

    var route = routeResult.Value;
    var created = await repository.AddAsync(route, cancellationToken);

    return new RouteResponse(created.Id, created.Name.Value, created.CreatedById.Value, created.CreatedAt, created.IsDeleted);
  }
}
