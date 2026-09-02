using BusStop.UseCases.Comments;
using BusStop.UseCases.Comments.Create;

namespace BusStop.Web.Comments;

public sealed class Create(IMediator mediator) : Endpoint<CreateCommentRequest, CommentResponse>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Post("/comments");
    Roles("RegisteredUser");
  }

  public override async Task HandleAsync(CreateCommentRequest req, CancellationToken ct)
  {
    var command = new CreateCommentCommand(req.Content, req.RouteId);
    var result = await _mediator.Send(command, ct);

    if (result.IsSuccess)
    {
      await Send.CreatedAtAsync<Create>(new { result.Value.Id }, result.Value, cancellation: ct);
      return;
    }

    if (result.Status == ResultStatus.NotFound)
      await Send.NotFoundAsync(ct);
    else if (result.Status == ResultStatus.Unauthorized)
      await Send.UnauthorizedAsync(ct);
    else
      await Send.ErrorsAsync(cancellation: ct);
  }
}

public sealed record CreateCommentRequest(string Content, long RouteId);

public sealed class CreateCommentValidator : Validator<CreateCommentRequest>
{
  public CreateCommentValidator()
  {
    RuleFor(x => x.Content).MaximumLength(2000);
  }
}
