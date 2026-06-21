using BusStop.UseCases.Routes;
using BusStop.UseCases.Routes.Create;

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

public sealed record CreateRequest(string Name, long CreatedById);

public sealed class CreateValidator : Validator<CreateRequest>
{
  public CreateValidator()
  {
    RuleFor(x => x.Name).MaximumLength(100);
  }
}
