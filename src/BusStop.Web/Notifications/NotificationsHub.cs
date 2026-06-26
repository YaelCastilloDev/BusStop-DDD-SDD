using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BusStop.Web.Notifications;

[Authorize]
public class NotificationsHub : Hub
{
  public const string ReceiveNotificationMethod = "ReceiveNotification";

  public override async Task OnConnectedAsync()
  {
    await base.OnConnectedAsync();
  }

  public override async Task OnDisconnectedAsync(Exception? exception)
  {
    await base.OnDisconnectedAsync(exception);
  }
}
