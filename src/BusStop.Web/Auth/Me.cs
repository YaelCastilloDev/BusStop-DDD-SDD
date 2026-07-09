using BusStop.UseCases.Users;
using BusStop.UseCases.Users.GetMe;
using BusStop.Web.Extensions;

namespace BusStop.Web.Auth;

public sealed class Me(IMediator mediator) : Endpoint<EmptyRequest, UserResponse>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/auth/me");
        Roles("RegisteredUser");
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
      var query = new GetMeQuery();
      var result = await _mediator.Send(query, ct);

      await this.ToGetByIdResultAsync(result, ct);
    }
}
