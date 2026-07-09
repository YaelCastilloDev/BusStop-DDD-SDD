using BusStop.UseCases.Comments;
using BusStop.UseCases.Comments.Create;
using BusStop.Web.Extensions;

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

    await this.ToCreatedResultAsync(result, new { result.Value.Id }, ct);
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
