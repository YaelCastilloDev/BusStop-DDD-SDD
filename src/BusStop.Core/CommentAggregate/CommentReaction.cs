using Ardalis.SharedKernel;
using BusStop.Core.UserAggregate;

namespace BusStop.Core.CommentAggregate;

public sealed class CommentReaction : ValueObject
{
  public UserId UserId { get; }
  public ReactionType ReactionType { get; }

  public CommentReaction(UserId userId, ReactionType reactionType)
  {
    UserId = userId;
    ReactionType = reactionType;
  }

#pragma warning disable CS8618
  private CommentReaction() { }
#pragma warning restore CS8618

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return UserId;
  }
}
