using Ardalis.Result;
using BusStop.Core.Interfaces;
using BusStop.UseCases.Users.Signup;

namespace BusStop.UnitTests.UseCases.Users.Signup;

public class SignupHandlerTests
{
    private readonly IKeycloakAdminService _keycloakAdmin;
    private readonly SignupHandler _handler;

    public SignupHandlerTests()
    {
        _keycloakAdmin = Substitute.For<IKeycloakAdminService>();
        _handler = new SignupHandler(_keycloakAdmin);
    }

    [Fact]
    public async Task Handle_CallsCreateUserAsync_WithCorrectEmailAndPassword()
    {
        // Arrange
        var command = new SignupCommand("test@example.com", "Password123!");
        _keycloakAdmin
            .CreateUserAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _keycloakAdmin
            .Received(1)
            .CreateUserAsync(command.Email, command.Password, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenCreateUserSucceeds()
    {
        // Arrange
        var command = new SignupCommand("test@example.com", "Password123!");
        _keycloakAdmin
            .CreateUserAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenCreateUserFails()
    {
        // Arrange
        var command = new SignupCommand("test@example.com", "Password123!");
        var expectedError = "User creation failed";
        _keycloakAdmin
            .CreateUserAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns(Result.Error(expectedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(expectedError);
    }
}
