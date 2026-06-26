using BusStop.UseCases.Comments.Moderate;

namespace BusStop.Web.Comments;

public sealed class Moderate(IMediator mediator) : Endpoint<ModerateCommentRequest>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Delete("/comments/{Id}");
    Roles("SubAdmin", "Admin");
  }

  public override async Task HandleAsync(ModerateCommentRequest req, CancellationToken ct)
  {
    var command = new ModerateCommentCommand(req.Id);
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

public sealed record ModerateCommentRequest(long Id);

public sealed class ModerateCommentValidator : Validator<ModerateCommentRequest>
{
}
