using BusStop.Core.ModerationActionAggregate;
using BusStop.UseCases.Routes.Moderate;
using BusStop.Web.Extensions;

namespace BusStop.Web.Routes;

public sealed class Moderate(IMediator mediator) : Endpoint<ModerateRouteRequest>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Patch("/routes/{Id}/moderate");
    Roles("SubAdmin", "Admin");
  }

  public override async Task HandleAsync(ModerateRouteRequest req, CancellationToken ct)
  {
    var command = new ModerateRouteCommand(req.Id, req.Category, req.Reason);
    var result = await _mediator.Send(command, ct);

    await this.ToNoContentResultAsync(result, ct);
  }
}

public sealed record ModerateRouteRequest(long Id, ModerationCategory Category, string Reason);

public sealed class ModerateRouteValidator : Validator<ModerateRouteRequest>
{
  public ModerateRouteValidator()
  {
    RuleFor(x => x.Id).GreaterThan(0);
    RuleFor(x => x.Category).IsInEnum();
    RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
  }
}
