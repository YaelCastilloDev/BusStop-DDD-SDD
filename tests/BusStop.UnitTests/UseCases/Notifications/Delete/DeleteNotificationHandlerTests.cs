using Ardalis.Result;
using Ardalis.SharedKernel;
using BusStop.Core.Interfaces;
using BusStop.Core.Notifications;
using BusStop.UseCases.Notifications.Delete;
using NSubstitute;

namespace BusStop.UnitTests.UseCases.Notifications.Delete;

// SPEC-NotificationContext-Moderation
public class DeleteNotificationHandlerTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUser _currentUser;
    private readonly DeleteNotificationHandler _handler;

    public DeleteNotificationHandlerTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();
        _currentUser = Substitute.For<ICurrentUser>();
        _handler = new DeleteNotificationHandler(_notificationRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_Succeeds_WhenOwner()
    {
        var command = new DeleteNotificationCommand(1) { Sub = "kc-sub" };
        _currentUser.Id.Returns(1L);

        var notification = new Notification(1, "Title", "Message");
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(notification, 100L);

        _notificationRepository.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
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

        _notificationRepository.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((Notification?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
        await _notificationRepository.DidNotReceive().DeleteAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenNotOwner()
    {
        var command = new DeleteNotificationCommand(1) { Sub = "kc-sub" };
        _currentUser.Id.Returns(1L);

        var notification = new Notification(99, "Title", "Message");
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(notification, 100L);

        _notificationRepository.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(notification);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Forbidden);
        await _notificationRepository.DidNotReceive().DeleteAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WithDescriptiveMessage()
    {
        var command = new DeleteNotificationCommand(99) { Sub = "kc-sub" };
        _currentUser.Id.Returns(1L);

        _notificationRepository.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns((Notification?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.NotFound);
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain("Notification not found.");
    }
}
