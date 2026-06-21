using BusStop.UseCases.Users;
using BusStop.UseCases.Users.GetById;

namespace BusStop.Web.Users;

public sealed class GetById(IMediator mediator) : Endpoint<GetByIdRequest, UserResponse>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Get("/users/{Id}");
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetByIdRequest req, CancellationToken ct)
  {
    var query = new GetUserByIdQuery(req.Id);
    var result = await _mediator.Send(query, ct);

    if (result.IsSuccess)
    {
      await Send.OkAsync(result.Value, ct);
      return;
    }

    if (result.Status == ResultStatus.NotFound)
      await Send.NotFoundAsync(ct);
    else
      await Send.ErrorsAsync(cancellation: ct);
  }
}

public sealed record GetByIdRequest(long Id);
