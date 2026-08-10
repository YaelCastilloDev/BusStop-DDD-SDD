using Ardalis.Result;
using Ardalis.SharedKernel;
using BusStop.Core.CommentAggregate;
using BusStop.Core.CommentAggregate.Specifications;
using BusStop.Core.Interfaces;
using BusStop.Core.ModerationActionAggregate;
using BusStop.Core.ModerationActionAggregate.Events;
using BusStop.Core.UserAggregate;
using BusStop.UseCases.Comments.Moderate;
using NSubstitute;

namespace BusStop.UnitTests.UseCases.Comments.Moderate;

// SPEC-TransitCatalog-ModerationAction
public class ModerateCommentHandlerTests
{
    private readonly IRepository<Comment> _commentRepository;
    private readonly IRepository<ModerationAction> _moderationActionRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ModerateCommentHandler _handler;

    public ModerateCommentHandlerTests()
    {
        _commentRepository = Substitute.For<IRepository<Comment>>();
        _moderationActionRepository = Substitute.For<IRepository<ModerationAction>>();
        _currentUser = Substitute.For<ICurrentUser>();
        _handler = new ModerateCommentHandler(_commentRepository, _moderationActionRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_Succeeds_WhenValidData()
    {
        var command = new ModerateCommentCommand(1, ModerationCategory.HateSpeech, "Hate speech detected") { Sub = "kc-sub" };
        _currentUser.Id.Returns(5L);

        var comment = Comment.Create("Some content", 5, 10).Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(comment, 1L);

        _commentRepository.FirstOrDefaultAsync(Arg.Any<CommentByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(comment);
        _moderationActionRepository.AddAsync(Arg.Any<ModerationAction>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<ModerationAction>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _moderationActionRepository.Received(1).AddAsync(Arg.Any<ModerationAction>(), Arg.Any<CancellationToken>());
        await _commentRepository.Received(1).UpdateAsync(comment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Succeeds_AndRegistersModerationActionRecordedEvent()
    {
        var command = new ModerateCommentCommand(1, ModerationCategory.HateSpeech, "Hate speech detected") { Sub = "kc-sub" };
        _currentUser.Id.Returns(5L);

        var comment = Comment.Create("Some content", 5, 10).Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(comment, 1L);

        _commentRepository.FirstOrDefaultAsync(Arg.Any<CommentByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        ModerationAction? capturedAction = null;
        _moderationActionRepository.AddAsync(Arg.Do<ModerationAction>(a => capturedAction = a), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<ModerationAction>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        capturedAction.ShouldNotBeNull();
        var domainEvent = capturedAction.DomainEvents.OfType<ModerationActionRecordedEvent>().Single();
        domainEvent.TargetType.ShouldBe(TargetType.Comment);
        domainEvent.TargetId.ShouldBe(comment.Id);
        domainEvent.UserId.ShouldBe(comment.UserId.Value);
        domainEvent.IssuedByUserId.ShouldBe(5);
        domainEvent.Category.ShouldBe(ModerationCategory.HateSpeech);
        domainEvent.Reason.ShouldBe("Hate speech detected");
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenCommentMissing()
    {
        var command = new ModerateCommentCommand(99, ModerationCategory.Spam, "Spam comment") { Sub = "kc-sub" };
        _currentUser.Id.Returns(5L);

        _commentRepository.FirstOrDefaultAsync(Arg.Any<CommentByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns((Comment?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
        await _moderationActionRepository.DidNotReceive().AddAsync(Arg.Any<ModerationAction>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserMissing()
    {
        var command = new ModerateCommentCommand(1, ModerationCategory.Spam, "Reason") { Sub = "unknown-sub" };
        _currentUser.Id.Returns(0L);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenModerationActionCreationFails()
    {
        var command = new ModerateCommentCommand(1, ModerationCategory.HateSpeech, "") { Sub = "kc-sub" };
        _currentUser.Id.Returns(5L);

        var comment = Comment.Create("Some content", 5, 10).Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(comment, 1L);

        _commentRepository.FirstOrDefaultAsync(Arg.Any<CommentByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        await _moderationActionRepository.DidNotReceive().AddAsync(Arg.Any<ModerationAction>(), Arg.Any<CancellationToken>());
        await _commentRepository.DidNotReceive().UpdateAsync(comment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotPersistModerationAction_WhenCommentUpdateFails()
    {
        var command = new ModerateCommentCommand(1, ModerationCategory.HateSpeech, "Hate speech detected") { Sub = "kc-sub" };
        _currentUser.Id.Returns(5L);

        var comment = Comment.Create("Some content", 5, 10).Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(comment, 1L);

        _commentRepository.FirstOrDefaultAsync(Arg.Any<CommentByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(comment);
        _moderationActionRepository.AddAsync(Arg.Any<ModerationAction>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<ModerationAction>());
        _commentRepository.When(x => x.UpdateAsync(comment, Arg.Any<CancellationToken>()))
            .Do(_ => throw new InvalidOperationException("Database failure"));

        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _handler.Handle(command, CancellationToken.None));

        exception.Message.ShouldBe("Database failure");

        await _moderationActionRepository.Received(1).AddAsync(Arg.Any<ModerationAction>(), Arg.Any<CancellationToken>());
    }
}
