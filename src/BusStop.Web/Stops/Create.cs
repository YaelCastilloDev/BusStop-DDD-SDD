using BusStop.UseCases.Stops;
using BusStop.UseCases.Stops.Create;
using BusStop.Web.Extensions;

namespace BusStop.Web.Stops;

public sealed class Create(IMediator mediator) : Endpoint<CreateStopRequest, StopResponse>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Post("/stops");
    Roles("RegisteredUser");
  }

  public override async Task HandleAsync(CreateStopRequest req, CancellationToken ct)
  {
    var command = new CreateStopCommand(req.Name, req.Latitude, req.Longitude, req.RouteId);
    var result = await _mediator.Send(command, ct);

    await this.ToCreatedResultAsync(result, new { result.Value.Id }, ct);
  }
}

public sealed record CreateStopRequest(string Name, double Latitude, double Longitude, long RouteId);

public sealed class CreateStopValidator : Validator<CreateStopRequest>
{
  public CreateStopValidator()
  {
    RuleFor(x => x.Name).MaximumLength(100);
  }
}
