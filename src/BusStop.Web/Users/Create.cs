using BusStop.UseCases.Users;
using BusStop.UseCases.Users.Create;

namespace BusStop.Web.Users;

public sealed class Create(IMediator mediator) : Endpoint<CreateUserRequest, UserResponse>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Post("/users");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
  {
    var command = new CreateUserCommand(req.Username, req.Email);
    var result = await _mediator.Send(command, ct);

    if (result.IsSuccess)
    {
      await Send.CreatedAtAsync<Create>(new { result.Value.Id }, result.Value, cancellation: ct);
      return;
    }

    await Send.ErrorsAsync(cancellation: ct);
  }
}

public sealed record CreateUserRequest(string Username, string Email);

public sealed class CreateUserValidator : Validator<CreateUserRequest>
{
  public CreateUserValidator()
  {
    RuleFor(x => x.Username).Must(x => !string.IsNullOrWhiteSpace(x)).MinimumLength(3).MaximumLength(50);
    RuleFor(x => x.Email).Must(x => !string.IsNullOrWhiteSpace(x)).EmailAddress();
  }
}
