using BusStop.UseCases.Notifications;
using BusStop.UseCases.Notifications.GetMy;
using BusStop.Web.Extensions;

namespace BusStop.Web.Notifications;

public sealed class GetMy(IMediator mediator) : EndpointWithoutRequest<IEnumerable<NotificationDto>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Get("/notifications");
    Roles("RegisteredUser");
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var query = new GetMyNotificationsQuery();
    var result = await _mediator.Send(query, ct);

    await this.ToOkResultAsync(result, ct);
  }
}
