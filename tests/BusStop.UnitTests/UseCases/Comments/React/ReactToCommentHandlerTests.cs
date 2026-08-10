using Ardalis.Result;
using Ardalis.SharedKernel;
using BusStop.Core.CommentAggregate;
using BusStop.Core.CommentAggregate.Specifications;
using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;
using BusStop.UseCases.Comments.React;

namespace BusStop.UnitTests.UseCases.Comments.React;

public class ReactToCommentHandlerTests
{
    private readonly IRepository<Comment> _repository;
    private readonly ICurrentUser _currentUser;
    private readonly ReactToCommentHandler _handler;

    public ReactToCommentHandlerTests()
    {
        _repository = Substitute.For<IRepository<Comment>>();
        _currentUser = Substitute.For<ICurrentUser>();
        _handler = new ReactToCommentHandler(_repository, _currentUser);
    }

    [Fact]
    public async Task Handle_ShouldCheck_AddReactionResult()
    {
        var command = new ReactToCommentCommand(1, true) { Sub = "kc-sub" };
        _currentUser.Id.Returns(1L);

        var comment = Comment.Create("Some content", 1, 10).Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(comment, 1L);

        _repository.FirstOrDefaultAsync(Arg.Any<CommentByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).UpdateAsync(comment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DislikeFalse_ShouldSucceed()
    {
        var command = new ReactToCommentCommand(1, false) { Sub = "kc-sub" };
        _currentUser.Id.Returns(1L);

        var comment = Comment.Create("Some content", 1, 10).Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(comment, 1L);

        _repository.FirstOrDefaultAsync(Arg.Any<CommentByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).UpdateAsync(comment, Arg.Any<CancellationToken>());
    }
}
