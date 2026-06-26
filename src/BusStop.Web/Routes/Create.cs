using BusStop.UseCases.Routes;
using BusStop.UseCases.Routes.Create;

namespace BusStop.Web.Routes;

public sealed class Create(IMediator mediator) : Endpoint<CreateRouteRequest, RouteResponse>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Post("/routes");
    Roles("RegisteredUser");
  }

  public override async Task HandleAsync(CreateRouteRequest req, CancellationToken ct)
  {
    var command = new CreateRouteCommand(req.Name);
    var result = await _mediator.Send(command, ct);

    if (result.IsSuccess)
    {
      await Send.CreatedAtAsync<Create>(new { result.Value.Id }, result.Value, cancellation: ct);
      return;
    }

    if (result.Status == ResultStatus.NotFound)
      await Send.NotFoundAsync(ct);
    else if (result.Status == ResultStatus.Unauthorized)
      await Send.UnauthorizedAsync(ct);
    else
      await Send.ErrorsAsync(cancellation: ct);
  }
}

public sealed record CreateRouteRequest(string Name);

public sealed class CreateRouteValidator : Validator<CreateRouteRequest>
{
  public CreateRouteValidator()
  {
    RuleFor(x => x.Name).MaximumLength(100);
  }
}
