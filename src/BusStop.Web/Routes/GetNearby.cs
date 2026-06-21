using BusStop.UseCases.Routes;
using BusStop.UseCases.Routes.GetNearby;

namespace BusStop.Web.Routes;

public sealed class GetNearby(IMediator mediator) : Endpoint<GetNearbyRequest, List<RouteResponse>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Get("/routes/nearby");
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetNearbyRequest req, CancellationToken ct)
  {
    var query = new GetNearbyRoutesQuery(req.Latitude, req.Longitude, req.RadiusKm ?? 10);
    var result = await _mediator.Send(query, ct);

    if (result.IsSuccess)
      await Send.OkAsync(result.Value, ct);
    else
      await Send.ErrorsAsync(cancellation: ct);
  }
}

public sealed record GetNearbyRequest(double Latitude, double Longitude, double? RadiusKm);
