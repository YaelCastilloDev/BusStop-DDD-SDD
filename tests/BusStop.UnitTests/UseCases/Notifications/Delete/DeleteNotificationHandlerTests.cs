using Ardalis.Result;
using Ardalis.SharedKernel;
using Ardalis.Specification;
using BusStop.Core.Interfaces;
using BusStop.Core.NotificationAggregate;
using BusStop.UseCases.Notifications.Delete;
using NSubstitute;

namespace BusStop.UnitTests.UseCases.Notifications.Delete;

// SPEC-NotificationContext-Moderation
public class DeleteNotificationHandlerTests
{
    private readonly IRepository<UserNotification> _notificationRepository;
    private readonly ICurrentUser _currentUser;
    private readonly DeleteNotificationHandler _handler;

    public DeleteNotificationHandlerTests()
    {
        _notificationRepository = Substitute.For<IRepository<UserNotification>>();
        _currentUser = Substitute.For<ICurrentUser>();
        _handler = new DeleteNotificationHandler(_notificationRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_Succeeds_WhenOwner()
    {
        var command = new DeleteNotificationCommand(1) { Sub = "kc-sub" };
        _currentUser.Id.Returns(1L);

        var notification = UserNotification.Create(1, "Title", "Message").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(notification, 100L);

        _notificationRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<UserNotification>>(), Arg.Any<CancellationToken>())
            .Returns(notification);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _notificationRepository.Received(1).DeleteAsync(notification, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenNotificationMissing()
    {
        var command = new DeleteNotificationCommand(99) { Sub = "kc-sub" };
        _currentUser.Id.Returns(1L);

        _notificationRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<UserNotification>>(), Arg.Any<CancellationToken>())
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
        _currentUser.Id.Returns(1L);

        var notification = UserNotification.Create(99, "Title", "Message").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(notification, 100L);

        _notificationRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<UserNotification>>(), Arg.Any<CancellationToken>())
            .Returns(notification);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Forbidden);
        await _notificationRepository.DidNotReceive().DeleteAsync(Arg.Any<UserNotification>(), Arg.Any<CancellationToken>());
    }
}
