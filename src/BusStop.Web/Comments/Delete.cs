using BusStop.UseCases.Comments.Delete;

namespace BusStop.Web.Comments;

public sealed class Delete(IMediator mediator) : Endpoint<DeleteCommentRequest>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Delete("/comments/{Id}");
    AllowAnonymous();
  }

  public override async Task HandleAsync(DeleteCommentRequest req, CancellationToken ct)
  {
    var command = new DeleteCommentCommand(req.Id, req.DeletedById);
    var result = await _mediator.Send(command, ct);

    if (result.IsSuccess)
    {
      await Send.NoContentAsync(ct);
      return;
    }

    if (result.Status == ResultStatus.NotFound)
      await Send.NotFoundAsync(ct);
    else
      await Send.ErrorsAsync(cancellation: ct);
  }
}

public sealed record DeleteCommentRequest(long Id, long DeletedById);

public sealed class DeleteCommentValidator : Validator<DeleteCommentRequest>
{
  public DeleteCommentValidator()
  {
    RuleFor(x => x.Id).Must(x => x > 0);
    RuleFor(x => x.DeletedById).Must(x => x > 0);
  }
}
