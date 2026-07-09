using BusStop.UseCases.Users.Signup;
using BusStop.Web.Extensions;

namespace BusStop.Web.Auth;

public sealed class Signup(IMediator mediator) : Endpoint<SignupRequest>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/auth/signup");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SignupRequest req, CancellationToken ct)
    {
      var command = new SignupCommand(req.Email, req.Password);
      var result = await _mediator.Send(command, ct);

      await this.ToNoContentResultAsync(result, ct);
    }
}

public sealed record SignupRequest(string Email, string Password);

public sealed class SignupValidator : Validator<SignupRequest>
{
    public SignupValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
    }
}
