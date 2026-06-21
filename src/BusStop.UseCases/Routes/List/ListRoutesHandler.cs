using BusStop.Core.RouteAggregate;
using Ardalis.Specification;

namespace BusStop.UseCases.Routes.List;

public sealed class ListRoutesHandler(IReadRepository<Route> repository) : IQueryHandler<ListRoutesQuery, Result<List<RouteResponse>>>
{
  public async ValueTask<Result<List<RouteResponse>>> Handle(ListRoutesQuery request, CancellationToken cancellationToken)
  {
    var spec = new ListRoutesSpec(request.Page, request.PageSize);
    var routes = await repository.ListAsync(spec, cancellationToken);

    var responses = routes.Select(r => new RouteResponse(r.Id, r.Name.Value, r.CreatedById.Value, r.CreatedAt, r.IsDeleted)).ToList();

    return responses;
  }

  private sealed class ListRoutesSpec : Specification<Route>
  {
    public ListRoutesSpec(int page, int pageSize)
    {
      Query
        .Where(r => !r.IsDeleted)
        .OrderByDescending(r => r.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize);
    }
  }
}

