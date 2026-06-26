using BusStop.UseCases.Comments.Delete;

namespace BusStop.Web.Comments;

public sealed class Delete(IMediator mediator) : Endpoint<DeleteCommentRequest>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Delete("/comments/{Id}");
    Roles("Curator");
  }

  public override async Task HandleAsync(DeleteCommentRequest req, CancellationToken ct)
  {
    var command = new DeleteCommentCommand(req.Id);
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

public sealed record DeleteCommentRequest(long Id);

public sealed class DeleteCommentValidator : Validator<DeleteCommentRequest>
{
}
