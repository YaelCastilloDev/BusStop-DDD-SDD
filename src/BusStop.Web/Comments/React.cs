using BusStop.UseCases.Comments.React;
using BusStop.Web.Extensions;

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

    await this.ToNoContentResultAsync(result, ct);
  }
}

public sealed record ReactToCommentRequest(long CommentId, string ReactionType);

public sealed class ReactToCommentValidator : Validator<ReactToCommentRequest>
{
  public ReactToCommentValidator()
  {
    RuleFor(x => x.CommentId).GreaterThan(0);
    RuleFor(x => x.ReactionType).Must(x => !string.IsNullOrWhiteSpace(x)).Must(x => x is "Like" or "Dislike");
  }
}
