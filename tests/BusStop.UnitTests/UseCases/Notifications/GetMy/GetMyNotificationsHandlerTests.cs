using Ardalis.Result;
using Ardalis.SharedKernel;
using BusStop.Core.Interfaces;
using BusStop.Core.Notifications;
using BusStop.UseCases.Notifications.GetMy;
using NSubstitute;

namespace BusStop.UnitTests.UseCases.Notifications.GetMy;

// SPEC-NotificationContext-Moderation
public class GetMyNotificationsHandlerTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUser _currentUser;
    private readonly GetMyNotificationsHandler _handler;

    public GetMyNotificationsHandlerTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();
        _currentUser = Substitute.For<ICurrentUser>();
        _handler = new GetMyNotificationsHandler(_notificationRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_ReturnsNotifications_ForCurrentUser()
    {
        var query = new GetMyNotificationsQuery { Sub = "kc-sub" };
        _currentUser.Id.Returns(1L);

        var notification = new Notification(1, "Title", "Message");
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(notification, 100L);

        _notificationRepository.GetByUserIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(new[] { notification });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count().ShouldBe(1);
        result.Value.First().Title.ShouldBe("Title");
        result.Value.First().Message.ShouldBe("Message");
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserMissing()
    {
        var query = new GetMyNotificationsQuery { Sub = "unknown-sub" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
}
