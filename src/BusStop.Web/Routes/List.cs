using BusStop.UseCases.Routes;
using BusStop.UseCases.Routes.List;
using BusStop.Web.Extensions;

namespace BusStop.Web.Routes;

public sealed class List(IMediator mediator) : Endpoint<ListRequest, List<RouteResponse>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Get("/routes");
    AllowAnonymous();
  }

  public override async Task HandleAsync(ListRequest req, CancellationToken ct)
  {
    var query = new ListRoutesQuery(req.Page ?? 1, req.PageSize ?? 20);
    var result = await _mediator.Send(query, ct);

    await this.ToOkResultAsync(result, ct);
  }
}

public sealed record ListRequest(int? Page, int? PageSize);
