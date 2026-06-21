using BusStop.Core.RouteAggregate;

namespace BusStop.UseCases.Routes.Create;

public sealed class CreateRouteHandler(IRepository<Route> repository) : ICommandHandler<CreateRouteCommand, Result<RouteResponse>>
{
  public async ValueTask<Result<RouteResponse>> Handle(CreateRouteCommand request, CancellationToken cancellationToken)
  {
    var result = Route.Create(request.Name, request.CreatedById);
    if (!result.IsSuccess)
      return Result<RouteResponse>.Error(result.Errors.FirstOrDefault());

    var route = result.Value;
    var created = await repository.AddAsync(route, cancellationToken);

    return new RouteResponse(created.Id, created.Name.Value, created.CreatedById.Value, created.CreatedAt, created.IsDeleted);
  }
}

