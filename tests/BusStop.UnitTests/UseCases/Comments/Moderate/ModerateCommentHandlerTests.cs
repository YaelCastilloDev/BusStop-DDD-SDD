using Ardalis.Result;
using Ardalis.SharedKernel;
using BusStop.Core.CommentAggregate;
using BusStop.Core.CommentAggregate.Specifications;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;
using BusStop.UseCases.Comments.Moderate;
using NSubstitute;

namespace BusStop.UnitTests.UseCases.Comments.Moderate;

public class ModerateCommentHandlerTests
{
    private readonly IRepository<Comment> _commentRepository;
    private readonly IReadRepository<User> _userRepository;
    private readonly ModerateCommentHandler _handler;

    public ModerateCommentHandlerTests()
    {
        _commentRepository = Substitute.For<IRepository<Comment>>();
        _userRepository = Substitute.For<IReadRepository<User>>();
        _handler = new ModerateCommentHandler(_commentRepository, _userRepository);
    }

    [Fact]
    public async Task Handle_Succeeds_WhenValidData()
    {
        var command = new ModerateCommentCommand(1) { Sub = "kc-sub" };
        var user = User.Create("mod@example.com", "kc-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(user, 5L);

        var comment = Comment.Create("Some content", user.Id, 10).Value;

        _userRepository.FirstOrDefaultAsync(Arg.Any<UserByExternalIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _commentRepository.FirstOrDefaultAsync(Arg.Any<CommentByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _commentRepository.Received(1).UpdateAsync(comment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenCommentMissing()
    {
        var command = new ModerateCommentCommand(99) { Sub = "kc-sub" };
        var user = User.Create("mod@example.com", "kc-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(user, 5L);

        _userRepository.FirstOrDefaultAsync(Arg.Any<UserByExternalIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _commentRepository.FirstOrDefaultAsync(Arg.Any<CommentByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns((Comment?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserMissing()
    {
        var command = new ModerateCommentCommand(1) { Sub = "unknown-sub" };

        _userRepository.FirstOrDefaultAsync(Arg.Any<UserByExternalIdSpec>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsUnauthorized_WhenNoSub()
    {
        var command = new ModerateCommentCommand(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Unauthorized);
    }
}
