using BusStop.UseCases.Routes.Delete;
using BusStop.Web.Extensions;

namespace BusStop.Web.Routes;

public sealed class Delete(IMediator mediator) : Endpoint<DeleteRouteRequest>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Delete("/routes/{Id}");
    Roles("Curator");
  }

  public override async Task HandleAsync(DeleteRouteRequest req, CancellationToken ct)
  {
    var command = new DeleteRouteCommand(req.Id);
    var result = await _mediator.Send(command, ct);

    await this.ToNoContentResultAsync(result, ct);
  }
}

public sealed record DeleteRouteRequest(long Id);
