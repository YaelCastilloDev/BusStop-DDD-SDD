using Ardalis.Result;
using Ardalis.SharedKernel;
using BusStop.Core.NotificationAggregate;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;
using BusStop.UseCases.Notifications.Delete;
using NSubstitute;

namespace BusStop.UnitTests.UseCases.Notifications.Delete;

// SPEC-NotificationContext-Moderation
public class DeleteNotificationHandlerTests
{
    private readonly IRepository<UserNotification> _notificationRepository;
    private readonly IReadRepository<User> _userRepository;
    private readonly DeleteNotificationHandler _handler;

    public DeleteNotificationHandlerTests()
    {
        _notificationRepository = Substitute.For<IRepository<UserNotification>>();
        _userRepository = Substitute.For<IReadRepository<User>>();
        _handler = new DeleteNotificationHandler(_notificationRepository, _userRepository);
    }

    [Fact]
    public async Task Handle_Succeeds_WhenOwner()
    {
        var command = new DeleteNotificationCommand(1) { Sub = "kc-sub" };
        var user = User.Create("test@example.com", "kc-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(user, 1L);

        var notification = UserNotification.Create(user.Id, "Title", "Message").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(notification, 100L);

        _userRepository.FirstOrDefaultAsync(Arg.Any<UserByExternalIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _notificationRepository.GetByIdAsync(command.NotificationId, Arg.Any<CancellationToken>())
            .Returns(notification);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _notificationRepository.Received(1).DeleteAsync(notification, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsUnauthorized_WhenNoSub()
    {
        var command = new DeleteNotificationCommand(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenNotificationMissing()
    {
        var command = new DeleteNotificationCommand(99) { Sub = "kc-sub" };
        var user = User.Create("test@example.com", "kc-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(user, 1L);

        _userRepository.FirstOrDefaultAsync(Arg.Any<UserByExternalIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _notificationRepository.GetByIdAsync(command.NotificationId, Arg.Any<CancellationToken>())
            .Returns((UserNotification?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
        await _notificationRepository.DidNotReceive().DeleteAsync(Arg.Any<UserNotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenNotOwner()
    {
        var command = new DeleteNotificationCommand(1) { Sub = "kc-sub" };
        var user = User.Create("test@example.com", "kc-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(user, 1L);

        var otherUser = User.Create("other@example.com", "kc-other").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(otherUser, 99L);

        var notification = UserNotification.Create(otherUser.Id, "Title", "Message").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(notification, 100L);

        _userRepository.FirstOrDefaultAsync(Arg.Any<UserByExternalIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _notificationRepository.GetByIdAsync(command.NotificationId, Arg.Any<CancellationToken>())
            .Returns(notification);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Forbidden);
        await _notificationRepository.DidNotReceive().DeleteAsync(Arg.Any<UserNotification>(), Arg.Any<CancellationToken>());
    }
}
