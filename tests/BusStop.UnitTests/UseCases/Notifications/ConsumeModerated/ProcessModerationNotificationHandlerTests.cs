using Ardalis.Result;
using Ardalis.SharedKernel;
using BusStop.Core.NotificationAggregate;
using BusStop.Core.NotificationAggregate.Interfaces;
using BusStop.Core.UserAggregate;
using BusStop.UseCases.Notifications.ConsumeModerated;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BusStop.UnitTests.UseCases.Notifications.ConsumeModerated;

// SPEC-NotificationContext-Moderation
public class ProcessModerationNotificationHandlerTests
{
    private readonly IRepository<UserNotification> _notificationRepository;
    private readonly IReadRepository<User> _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly ProcessModerationNotificationHandler _handler;

    public ProcessModerationNotificationHandlerTests()
    {
        _notificationRepository = Substitute.For<IRepository<UserNotification>>();
        _userRepository = Substitute.For<IReadRepository<User>>();
        _emailSender = Substitute.For<IEmailSender>();
        var logger = Substitute.For<ILogger<ProcessModerationNotificationHandler>>();
        _handler = new ProcessModerationNotificationHandler(_notificationRepository, _userRepository, _emailSender, logger);
    }

    [Fact]
    public async Task Handle_Succeeds_WhenValidData()
    {
        var command = new ProcessModerationNotificationCommand(1, 42, "Inappropriate content");
        var user = User.Create("test@example.com", "kc-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(user, 1L);

        _userRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);
        _notificationRepository.AddAsync(Arg.Any<UserNotification>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<UserNotification>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _notificationRepository.Received(1).AddAsync(Arg.Any<UserNotification>(), Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendEmailAsync(user.Email, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserMissing()
    {
        var command = new ProcessModerationNotificationCommand(99, 42, "reason");

        _userRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
        await _notificationRepository.DidNotReceive().AddAsync(Arg.Any<UserNotification>(), Arg.Any<CancellationToken>());
        await _emailSender.DidNotReceive().SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenZeroUserId()
    {
        var command = new ProcessModerationNotificationCommand(0, 42, "reason");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
        await _notificationRepository.DidNotReceive().AddAsync(Arg.Any<UserNotification>(), Arg.Any<CancellationToken>());
    }
}
