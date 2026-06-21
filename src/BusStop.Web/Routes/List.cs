using BusStop.UseCases.Routes;
using BusStop.UseCases.Routes.List;

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

    if (result.IsSuccess)
      await Send.OkAsync(result.Value, ct);
    else
      await Send.ErrorsAsync(cancellation: ct);
  }
}

public sealed record ListRequest(int? Page, int? PageSize);
