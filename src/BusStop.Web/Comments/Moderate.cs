using BusStop.Core.ModerationActionAggregate;
using BusStop.UseCases.Comments.Moderate;
using BusStop.Web.Extensions;

namespace BusStop.Web.Comments;

public sealed class Moderate(IMediator mediator) : Endpoint<ModerateCommentRequest>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Patch("/comments/{Id}/moderate");
    Roles("SubAdmin", "Admin");
  }

  public override async Task HandleAsync(ModerateCommentRequest req, CancellationToken ct)
  {
    var command = new ModerateCommentCommand(req.Id, req.Category, req.Reason);
    var result = await _mediator.Send(command, ct);

    await this.ToNoContentResultAsync(result, ct);
  }
}

public sealed record ModerateCommentRequest(long Id, ModerationCategory Category, string Reason);

public sealed class ModerateCommentValidator : Validator<ModerateCommentRequest>
{
  public ModerateCommentValidator()
  {
    RuleFor(x => x.Id).GreaterThan(0);
    RuleFor(x => x.Category).IsInEnum();
    RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
  }
}
