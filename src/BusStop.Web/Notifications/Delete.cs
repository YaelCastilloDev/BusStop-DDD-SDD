using BusStop.UseCases.Notifications.Delete;
using BusStop.Web.Extensions;

namespace BusStop.Web.Notifications;

public sealed record DeleteNotificationRequest(long Id);

public sealed class Delete(IMediator mediator) : Endpoint<DeleteNotificationRequest>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Delete("/notifications/{id}");
    Roles("RegisteredUser");
  }

  public override async Task HandleAsync(DeleteNotificationRequest req, CancellationToken ct)
  {
    var command = new DeleteNotificationCommand(req.Id);
    var result = await _mediator.Send(command, ct);

    await this.ToNoContentResultAsync(result, ct);
  }
}
