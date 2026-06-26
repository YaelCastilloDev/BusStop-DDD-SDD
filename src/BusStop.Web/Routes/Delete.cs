using BusStop.UseCases.Routes.Delete;

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

    if (result.IsSuccess)
    {
      await Send.NoContentAsync(ct);
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

public sealed record DeleteRouteRequest(long Id);
