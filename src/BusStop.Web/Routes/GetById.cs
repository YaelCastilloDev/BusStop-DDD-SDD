using BusStop.UseCases.Routes;
using BusStop.UseCases.Routes.GetById;
using BusStop.Web.Extensions;

namespace BusStop.Web.Routes;

public sealed class GetById(IMediator mediator) : Endpoint<GetByIdRequest, RouteResponse>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Get("/routes/{Id}");
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetByIdRequest req, CancellationToken ct)
  {
    var query = new GetRouteByIdQuery(req.Id);
    var result = await _mediator.Send(query, ct);

    await this.ToGetByIdResultAsync(result, ct);
  }
}

public sealed record GetByIdRequest(long Id);

public sealed class GetRouteByIdValidator : Validator<GetByIdRequest>
{
  public GetRouteByIdValidator()
  {
    RuleFor(x => x.Id).GreaterThan(0);
  }
}
