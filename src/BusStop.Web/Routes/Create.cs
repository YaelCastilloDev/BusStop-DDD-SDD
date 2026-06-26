using BusStop.UseCases.Routes;
using BusStop.UseCases.Routes.Create;
using BusStop.Web.Extensions;

namespace BusStop.Web.Routes;

public sealed class Create(IMediator mediator) : Endpoint<CreateRequest, RouteResponse>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Post("/routes");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CreateRequest req, CancellationToken ct)
  {
    var command = new CreateRouteCommand(req.Name, req.CreatedById);
    var result = await _mediator.Send(command, ct);

    await this.ToCreatedResultAsync(result, new { id = result.Value?.Id }, ct);
  }
}

public sealed record CreateRequest(string Name, long CreatedById);

public sealed class CreateValidator : Validator<CreateRequest>
{
  public CreateValidator()
  {
    RuleFor(x => x.Name).MaximumLength(100);
  }
}
