using BusStop.UseCases.Routes.Delete;
using BusStop.Web.Extensions;

namespace BusStop.Web.Routes;

public sealed class Delete(IMediator mediator) : Endpoint<DeleteRequest>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Delete("/routes/{Id}");
    AllowAnonymous();
  }

  public override async Task HandleAsync(DeleteRequest req, CancellationToken ct)
  {
    var command = new DeleteRouteCommand(req.Id, req.DeletedById);
    var result = await _mediator.Send(command, ct);

    await this.ToNoContentResultAsync(result, ct);
  }
}

public sealed record DeleteRequest(long Id, long DeletedById);

public sealed class DeleteValidator : Validator<DeleteRequest>
{
}
