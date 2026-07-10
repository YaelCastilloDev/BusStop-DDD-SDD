using BusStop.UseCases.Routes.GetNearby;
using BusStop.Web.Extensions;

namespace BusStop.Web.Routes;

public sealed class GetNearby(IMediator mediator) : Endpoint<GetNearbyRequest, NearbyRoutesResponse>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Get("/routes/nearby");
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetNearbyRequest req, CancellationToken ct)
  {
    var query = new GetNearbyRoutesQuery(req.Latitude, req.Longitude);
    var result = await _mediator.Send(query, ct);

    var mappedResult = result.Map(r => new NearbyRoutesResponse(
        r.Routes,
        r.IsClosestMatchOnly,
        r.Message));

    await this.ToOkResultAsync(mappedResult, ct);
  }
}

public sealed record GetNearbyRequest(double Latitude, double Longitude);

public sealed record NearbyRoutesResponse(
    List<NearbyRouteDto> Routes,
    bool IsClosestMatchOnly,
    string Message);

public sealed class GetNearbyValidator : Validator<GetNearbyRequest>
{
  public GetNearbyValidator()
  {
    RuleFor(x => x.Latitude)
      .InclusiveBetween(-90, 90)
      .WithMessage("Latitude must be between -90 and 90.");

    RuleFor(x => x.Longitude)
      .InclusiveBetween(-180, 180)
      .WithMessage("Longitude must be between -180 and 180.");
  }
}
