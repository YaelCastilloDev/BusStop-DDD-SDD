using BusStop.UseCases.Comments.React;

namespace BusStop.Web.Comments;

public sealed class React(IMediator mediator) : Endpoint<ReactToCommentRequest>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Post("/comments/react");
    Roles("RegisteredUser");
  }

  public override async Task HandleAsync(ReactToCommentRequest req, CancellationToken ct)
  {
    var command = new ReactToCommentCommand(req.CommentId, req.ReactionType);
    var result = await _mediator.Send(command, ct);

    if (result.IsSuccess)
    {
      await Send.NoContentAsync(ct);
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

public sealed record ReactToCommentRequest(long CommentId, string ReactionType);

public sealed class ReactToCommentValidator : Validator<ReactToCommentRequest>
{
  public ReactToCommentValidator()
  {
    RuleFor(x => x.ReactionType).Must(x => !string.IsNullOrWhiteSpace(x)).Must(x => x is "Like" or "Dislike");
  }
}
