using Ardalis.Result;
using Ardalis.SharedKernel;
using BusStop.Core.Errors;
using BusStop.Core.Interfaces;
using BusStop.Core.UserAggregate;
using BusStop.Core.UserAggregate.Specifications;
using BusStop.UseCases;
using BusStop.UseCases.Users;
using BusStop.UseCases.Users.UpdateUsername;

namespace BusStop.UnitTests.UseCases.Users.UpdateUsername;

public class UpdateUsernameHandlerTests
{
    private readonly IRepository<User> _repository;
    private readonly ICurrentUser _currentUser;
    private readonly UpdateUsernameHandler _handler;

    public UpdateUsernameHandlerTests()
    {
        _repository = Substitute.For<IRepository<User>>();
        _currentUser = Substitute.For<ICurrentUser>();
        _handler = new UpdateUsernameHandler(_repository, _currentUser);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserNotAuthenticated()
    {
        _currentUser.Id.Returns(0L);
        var command = new UpdateUsernameCommand("new_username") { Sub = "kc-sub" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenUserDoesNotExist()
    {
        _currentUser.Id.Returns(1L);
        _repository.FirstOrDefaultAsync(Arg.Any<UserByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        var command = new UpdateUsernameCommand("new_username") { Sub = "kc-sub" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenUsernameAlreadyTaken()
    {
        var currentUser = User.Create("test@example.com", "keycloak-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(currentUser, 1L);

        var otherUser = User.Create("other@example.com", "other-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(otherUser, 2L);
        otherUser.CompleteOnboarding(new Username("taken_name"), 1);

        _currentUser.Id.Returns(1L);
        _repository.FirstOrDefaultAsync(Arg.Any<UserByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(currentUser);
        _repository.FirstOrDefaultAsync(Arg.Any<UserByUsernameSpec>(), Arg.Any<CancellationToken>())
            .Returns(otherUser);

        var command = new UpdateUsernameCommand("taken_name") { Sub = "kc-sub" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains(UserErrors.UsernameAlreadyTaken));
    }

    [Fact]
    public async Task Handle_Succeeds_WhenSameUserHasSameUsername()
    {
        var currentUser = User.Create("test@example.com", "keycloak-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(currentUser, 1L);
        currentUser.CompleteOnboarding(new Username("my_name"), 1);

        _currentUser.Id.Returns(1L);
        _repository.FirstOrDefaultAsync(Arg.Any<UserByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(currentUser);
        _repository.FirstOrDefaultAsync(Arg.Any<UserByUsernameSpec>(), Arg.Any<CancellationToken>())
            .Returns(currentUser);

        var command = new UpdateUsernameCommand("my_name") { Sub = "kc-sub" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).UpdateAsync(currentUser, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Succeeds_WhenNewUniqueUsername()
    {
        var currentUser = User.Create("test@example.com", "keycloak-sub").Value;
        typeof(EntityBase<long>).GetProperty("Id")!.SetValue(currentUser, 1L);
        currentUser.CompleteOnboarding(new Username("old_name"), 1);

        _currentUser.Id.Returns(1L);
        _repository.FirstOrDefaultAsync(Arg.Any<UserByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(currentUser);
        _repository.FirstOrDefaultAsync(Arg.Any<UserByUsernameSpec>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new UpdateUsernameCommand("new_unique") { Sub = "kc-sub" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Username.ShouldBe("new_unique");
        await _repository.Received(1).UpdateAsync(currentUser, Arg.Any<CancellationToken>());
    }
}
