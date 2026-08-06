using Ardalis.Result;
using Ardalis.SharedKernel;
using Ardalis.Specification;
using BusStop.Core.Interfaces;
using BusStop.Core.ModerationActionAggregate;
using BusStop.Core.Notifications;
using BusStop.Core.UserAggregate;
using BusStop.UseCases.Notifications.ConsumeModerated;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BusStop.UnitTests.UseCases.Notifications.ConsumeModerated;

// SPEC-TransitCatalog-ModerationAction
public class ProcessModerationNotificationHandlerTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IReadRepository<User> _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly ProcessModerationNotificationHandler _handler;

    public ProcessModerationNotificationHandlerTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();
        _userRepository = Substitute.For<IReadRepository<User>>();
        _emailSender = Substitute.For<IEmailSender>();
        var logger = Substitute.For<ILogger<ProcessModerationNotificationHandler>>();
        _handler = new ProcessModerationNotificationHandler(_notificationRepository, _userRepository, _emailSender, logger);
    }

    [Fact]
    public async Task Handle_Succeeds_WhenValidData()
    {
        var command = new ProcessModerationNotificationCommand(1, TargetType.Comment, 42, "Inappropriate content", ModerationCategory.HateSpeech);
        var user = User.Create("test@example.com", "kc-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(user, 1L);

        _userRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _notificationRepository.AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _notificationRepository.Received(1).AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendEmailAsync(user.Email, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotificationMessageIncludesCategoryAndTargetType()
    {
        var command = new ProcessModerationNotificationCommand(1, TargetType.Route, 42, "Inappropriate route", ModerationCategory.InappropriateContent);
        var user = User.Create("test@example.com", "kc-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(user, 1L);

        _userRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _notificationRepository.AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _emailSender.Received(1).SendEmailAsync(
            user.Email,
            Arg.Is<string>(title => title.Contains("Route", StringComparison.OrdinalIgnoreCase)),
            Arg.Is<string>(message => message.Contains("InappropriateContent", StringComparison.OrdinalIgnoreCase) && message.Contains("Inappropriate route")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserMissing()
    {
        var command = new ProcessModerationNotificationCommand(99, TargetType.Comment, 42, "reason", ModerationCategory.Spam);

        _userRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
        await _notificationRepository.DidNotReceive().AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        await _emailSender.DidNotReceive().SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenZeroUserId()
    {
        var command = new ProcessModerationNotificationCommand(0, TargetType.Comment, 42, "reason", ModerationCategory.Spam);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
        await _notificationRepository.DidNotReceive().AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotificationPersisted_BeforeEmail_NoCompensationOnFailure()
    {
        var notificationRepo = Substitute.For<INotificationRepository>();
        var userRepo = Substitute.For<IReadRepository<User>>();
        var emailSender = Substitute.For<IEmailSender>();
        var logger = Substitute.For<ILogger<ProcessModerationNotificationHandler>>();
        var handler = new ProcessModerationNotificationHandler(notificationRepo, userRepo, emailSender, logger);

        var command = new ProcessModerationNotificationCommand(1, TargetType.Comment, 42, "Inappropriate", ModerationCategory.HateSpeech);
        var user = User.Create("test@example.com", "kc-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(user, 1L);

        userRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);
        notificationRepo.AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        emailSender.SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Email service down")));

        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await handler.Handle(command, CancellationToken.None));

        exception.Message.ShouldBe("Email service down");

        await notificationRepo.Received(1).AddAsync(
            Arg.Any<Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Succeeds_WhenEmailDeliveryWorks()
    {
        var notificationRepo = Substitute.For<INotificationRepository>();
        var userRepo = Substitute.For<IReadRepository<User>>();
        var emailSender = Substitute.For<IEmailSender>();
        var logger = Substitute.For<ILogger<ProcessModerationNotificationHandler>>();
        var handler = new ProcessModerationNotificationHandler(notificationRepo, userRepo, emailSender, logger);

        var command = new ProcessModerationNotificationCommand(1, TargetType.Comment, 42, "Inappropriate", ModerationCategory.HateSpeech);
        var user = User.Create("test@example.com", "kc-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(user, 1L);

        userRepo.FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);
        notificationRepo.AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await notificationRepo.Received(1).AddAsync(
            Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        await emailSender.Received(1).SendEmailAsync(
            user.Email, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
