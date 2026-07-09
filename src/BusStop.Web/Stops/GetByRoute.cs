using BusStop.UseCases.Stops;
using BusStop.UseCases.Stops.GetByRoute;
using BusStop.Web.Extensions;

namespace BusStop.Web.Stops;

public sealed class GetByRoute(IMediator mediator) : Endpoint<GetStopsByRouteRequest, List<StopResponse>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Get("/stops/route/{RouteId}");
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetStopsByRouteRequest req, CancellationToken ct)
  {
    var query = new GetStopsByRouteQuery(req.RouteId);
    var result = await _mediator.Send(query, ct);

    await this.ToOkResultAsync(result, ct);
  }
}

public sealed record GetStopsByRouteRequest(long RouteId);
