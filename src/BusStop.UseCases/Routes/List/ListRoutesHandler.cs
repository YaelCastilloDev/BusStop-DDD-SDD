using Ardalis.Specification;
using BusStop.Core.RouteAggregate;

namespace BusStop.UseCases.Routes.List;

public sealed class ListRoutesHandler(IReadRepository<Route> repository) : IQueryHandler<ListRoutesQuery, Result<List<RouteResponse>>>
{
  public async ValueTask<Result<List<RouteResponse>>> Handle(ListRoutesQuery request, CancellationToken cancellationToken)
  {
    var spec = new ListRoutesSpec(request.Page, request.PageSize);
    var routes = await repository.ListAsync(spec, cancellationToken);

    var responses = routes.Select(r => r.ToResponse()).ToList();

    return responses;
  }

  private sealed class ListRoutesSpec : Specification<Route>
  {
    public ListRoutesSpec(int page, int pageSize)
    {
      Query
        .OrderByDescending(r => r.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize);
    }
  }
}

