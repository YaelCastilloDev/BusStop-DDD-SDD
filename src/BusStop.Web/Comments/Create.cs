using BusStop.UseCases.Comments;
using BusStop.UseCases.Comments.Create;

namespace BusStop.Web.Comments;

public sealed class Create(IMediator mediator) : Endpoint<CreateCommentRequest, CommentResponse>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Post("/comments");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CreateCommentRequest req, CancellationToken ct)
  {
    var command = new CreateCommentCommand(req.Content, req.UserId, req.RouteId);
    var result = await _mediator.Send(command, ct);

    if (result.IsSuccess)
    {
      await Send.CreatedAtAsync<Create>(new { result.Value.Id }, result.Value, cancellation: ct);
      return;
    }

    await Send.ErrorsAsync(cancellation: ct);
  }
}

public sealed record CreateCommentRequest(string Content, long UserId, long RouteId);

public sealed class CreateCommentValidator : Validator<CreateCommentRequest>
{
  public CreateCommentValidator()
  {
    RuleFor(x => x.Content).Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(2000);
    RuleFor(x => x.UserId).Must(x => x > 0);
    RuleFor(x => x.RouteId).Must(x => x > 0);
  }
}
