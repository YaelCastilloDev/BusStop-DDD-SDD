using Ardalis.Result;
using Ardalis.SharedKernel;
using BusStop.Core.NotificationAggregate;
using BusStop.Core.NotificationAggregate.Specifications;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;
using BusStop.UseCases.Notifications;
using BusStop.UseCases.Notifications.GetMy;
using NSubstitute;

namespace BusStop.UnitTests.UseCases.Notifications.GetMy;

// SPEC-NotificationContext-Moderation
public class GetMyNotificationsHandlerTests
{
    private readonly IReadRepository<UserNotification> _notificationRepository;
    private readonly IReadRepository<User> _userRepository;
    private readonly GetMyNotificationsHandler _handler;

    public GetMyNotificationsHandlerTests()
    {
        _notificationRepository = Substitute.For<IReadRepository<UserNotification>>();
        _userRepository = Substitute.For<IReadRepository<User>>();
        _handler = new GetMyNotificationsHandler(_notificationRepository, _userRepository);
    }

    [Fact]
    public async Task Handle_ReturnsNotifications_ForCurrentUser()
    {
        var query = new GetMyNotificationsQuery { Sub = "kc-sub" };
        var user = User.Create("test@example.com", "kc-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(user, 1L);

        var notification = UserNotification.Create(user.Id, "Title", "Message").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(notification, 100L);

        _userRepository.FirstOrDefaultAsync(Arg.Any<UserByExternalIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _notificationRepository.ListAsync(Arg.Any<NotificationsByUserIdSpec>(), Arg.Any<CancellationToken>())
            .Returns([notification]);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count().ShouldBe(1);
        result.Value.First().Title.ShouldBe("Title");
        result.Value.First().Message.ShouldBe("Message");
    }

    [Fact]
    public async Task Handle_ReturnsUnauthorized_WhenNoSub()
    {
        var query = new GetMyNotificationsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserMissing()
    {
        var query = new GetMyNotificationsQuery { Sub = "unknown-sub" };

        _userRepository.FirstOrDefaultAsync(Arg.Any<UserByExternalIdSpec>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
}
