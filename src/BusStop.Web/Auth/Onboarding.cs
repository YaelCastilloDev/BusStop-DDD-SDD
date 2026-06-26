using BusStop.UseCases.Users;
using BusStop.UseCases.Users.Register;

namespace BusStop.Web.Auth;

public sealed class Onboarding(IMediator mediator) : Endpoint<OnboardingRequest, UserResponse>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Post("/auth/onboarding");
    Roles("RegisteredUser");
  }

  public override async Task HandleAsync(OnboardingRequest req, CancellationToken ct)
  {
    var command = new OnboardingCommand(req.Username, req.CountryId);
    var result = await _mediator.Send(command, ct);

    if (result.IsSuccess)
    {
      await Send.OkAsync(result.Value, ct);
      return;
    }

    if (result.Status == ResultStatus.Unauthorized)
    {
      await Send.UnauthorizedAsync(ct);
      return;
    }

    if (result.Status == ResultStatus.NotFound)
    {
      await Send.NotFoundAsync(ct);
      return;
    }

    await Send.ErrorsAsync(cancellation: ct);
  }
}

public sealed record OnboardingRequest(string Username, long CountryId);

public sealed class OnboardingValidator : Validator<OnboardingRequest>
{
  public OnboardingValidator()
  {
    RuleFor(x => x.Username).MinimumLength(3).MaximumLength(50);
    RuleFor(x => x.CountryId).GreaterThan(0);
  }
}
