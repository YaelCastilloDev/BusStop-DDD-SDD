using BusStop.UseCases.Stops;
using BusStop.UseCases.Stops.Create;

namespace BusStop.Web.Stops;

public sealed class Create(IMediator mediator) : Endpoint<CreateStopRequest, StopResponse>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Post("/stops");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CreateStopRequest req, CancellationToken ct)
  {
    var command = new CreateStopCommand(req.Name, req.Latitude, req.Longitude, req.RouteId);
    var result = await _mediator.Send(command, ct);

    if (result.IsSuccess)
    {
      await Send.CreatedAtAsync<Create>(new { result.Value.Id }, result.Value, cancellation: ct);
      return;
    }

    if (result.Status == ResultStatus.NotFound)
      await Send.NotFoundAsync(ct);
    else
      await Send.ErrorsAsync(cancellation: ct);
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
