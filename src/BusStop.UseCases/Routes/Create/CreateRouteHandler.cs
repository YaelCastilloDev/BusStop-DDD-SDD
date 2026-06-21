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
    var user = await userRepository.FirstOrDefaultAsync(new UserByIdSpec(new UserId(request.CreatedById)), cancellationToken);
    if (user is null)
      return Result<RouteResponse>.NotFound("User not found.");

    var result = Route.Create(request.Name, request.CreatedById);
    if (!result.IsSuccess)
      return Result<RouteResponse>.Error(result.Errors.FirstOrDefault());

    var route = result.Value;
    var created = await repository.AddAsync(route, cancellationToken);

    return new RouteResponse(created.Id, created.Name.Value, created.CreatedById.Value, created.CreatedAt, created.IsDeleted);
  }
}

