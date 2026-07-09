using BusStop.UseCases.Comments;
using BusStop.UseCases.Comments.GetByRoute;
using BusStop.Web.Extensions;

namespace BusStop.Web.Comments;

public sealed class GetByRoute(IMediator mediator) : Endpoint<GetCommentsByRouteRequest, List<CommentResponse>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Get("/comments/route/{RouteId}");
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetCommentsByRouteRequest req, CancellationToken ct)
  {
    var query = new GetCommentsByRouteQuery(req.RouteId);
    var result = await _mediator.Send(query, ct);

    await this.ToOkResultAsync(result, ct);
  }
}

public sealed record GetCommentsByRouteRequest(long RouteId);
