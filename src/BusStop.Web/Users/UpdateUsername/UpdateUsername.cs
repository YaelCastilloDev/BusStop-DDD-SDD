using BusStop.UseCases.Users;
using BusStop.UseCases.Users.UpdateUsername;
using BusStop.Web.Extensions;

namespace BusStop.Web.Users.UpdateUsername;

public sealed class UpdateUsername(IMediator mediator) : Endpoint<UpdateUsernameRequest, UserResponse>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Patch("/users/username");
    Roles("RegisteredUser");
    Description(x => x.WithTags("Users"));
    Summary(s =>
    {
      s.Summary = "Update the current user's username";
      s.Description = "Allows a registered user to change their username. The new username must be between 3 and 50 characters.";
    });
  }

  public override async Task HandleAsync(UpdateUsernameRequest req, CancellationToken ct)
  {
    var command = new UpdateUsernameCommand(req.NewUsername);
    var result = await _mediator.Send(command, ct);

    await this.ToOkResultAsync(result, ct);
  }
}

public sealed record UpdateUsernameRequest(string NewUsername);

public sealed class UpdateUsernameValidator : Validator<UpdateUsernameRequest>
{
  public UpdateUsernameValidator()
  {
    RuleFor(x => x.NewUsername).MinimumLength(3).MaximumLength(50);
  }
}
