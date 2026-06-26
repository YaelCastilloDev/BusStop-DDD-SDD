using BusStop.UseCases.Routes;
using BusStop.UseCases.Routes.Create;
using BusStop.Web.Extensions;

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

    await this.ToCreatedResultAsync(result, new { id = result.Value?.Id }, ct);
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
