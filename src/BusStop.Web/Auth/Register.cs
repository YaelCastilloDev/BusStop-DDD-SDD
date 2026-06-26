using BusStop.UseCases.Users;
using BusStop.UseCases.Users.Register;

namespace BusStop.Web.Auth;

public sealed class Register(IMediator mediator) : Endpoint<RegisterUserRequest, UserResponse>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Post("/auth/register");
    Roles("RegisteredUser");
  }

  public override async Task HandleAsync(RegisterUserRequest req, CancellationToken ct)
  {
    var command = new RegisterUserCommand(req.Email);
    var result = await _mediator.Send(command, ct);

    if (result.IsSuccess)
    {
      await Send.CreatedAtAsync<Register>(new { result.Value.Id }, result.Value, cancellation: ct);
      return;
    }

    if (result.Status == ResultStatus.Unauthorized)
    {
      await Send.UnauthorizedAsync(ct);
      return;
    }

    await Send.ErrorsAsync(cancellation: ct);
  }
}

public sealed record RegisterUserRequest(string Email);

public sealed class RegisterUserValidator : Validator<RegisterUserRequest>
{
  public RegisterUserValidator()
  {
    RuleFor(x => x.Email).EmailAddress();
  }
}
