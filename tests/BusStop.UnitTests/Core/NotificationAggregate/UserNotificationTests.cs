using Ardalis.Result;
using BusStop.Core.Errors;
using BusStop.Core.NotificationAggregate;

namespace BusStop.UnitTests.Core.NotificationAggregate;

public class UserNotificationTests
{
    [Fact]
    public void Create_ReturnsSuccess_WhenValidInput()
    {
        var result = UserNotification.Create(1, "Test Title", "Test Message");

        result.IsSuccess.ShouldBeTrue();
        result.Value.UserId.Value.ShouldBe(1);
        result.Value.Title.ShouldBe("Test Title");
        result.Value.Message.ShouldBe("Test Message");
        result.Value.IsRead.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ReturnsError_WhenInvalidUserId(long userId)
    {
        var result = UserNotification.Create(userId, "Test Title", "Test Message");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(NotificationErrors.InvalidUserId));
    }

    [Fact]
    public void Create_ReturnsError_WhenEmptyTitle()
    {
        var result = UserNotification.Create(1, "", "Test Message");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(NotificationErrors.EmptyTitle));
    }

    [Fact]
    public void Create_ReturnsError_WhenWhitespaceTitle()
    {
        var result = UserNotification.Create(1, "   ", "Test Message");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(NotificationErrors.EmptyTitle));
    }

    [Fact]
    public void Create_ReturnsError_WhenEmptyMessage()
    {
        var result = UserNotification.Create(1, "Test Title", "");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(NotificationErrors.EmptyMessage));
    }

    [Fact]
    public void Create_ReturnsError_WhenWhitespaceMessage()
    {
        var result = UserNotification.Create(1, "Test Title", "   ");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(NotificationErrors.EmptyMessage));
    }

    [Fact]
    public void Create_ReturnsMultipleErrors_WhenMultipleInvalidInputs()
    {
        var result = UserNotification.Create(0, "", "");

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(NotificationErrors.InvalidUserId));
        result.Errors.ShouldContain(e => e.Contains(NotificationErrors.EmptyTitle));
        result.Errors.ShouldContain(e => e.Contains(NotificationErrors.EmptyMessage));
    }

    [Fact]
    public void MarkAsRead_SetsIsReadTrue_WhenNotRead()
    {
        var notificationResult = UserNotification.Create(1, "Test Title", "Test Message");
        var notification = notificationResult.Value;

        notification.MarkAsRead();

        notification.IsRead.ShouldBeTrue();
    }

    [Fact]
    public void MarkAsRead_StaysTrue_WhenCalledTwice()
    {
        var notificationResult = UserNotification.Create(1, "Test Title", "Test Message");
        var notification = notificationResult.Value;
        notification.MarkAsRead();

        notification.MarkAsRead();

        notification.IsRead.ShouldBeTrue();
    }
}
