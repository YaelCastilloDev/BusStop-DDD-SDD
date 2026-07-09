using Ardalis.SharedKernel;
using BusStop.Core.Exceptions;
using BusStop.Core.UserAggregate;

namespace BusStop.Core.CommentAggregate;

public sealed class CommentReaction : ValueObject
{
  public UserId UserId { get; }
  public ReactionType ReactionType { get; }

  private CommentReaction(UserId userId, ReactionType reactionType)
  {
    UserId = userId;
    ReactionType = reactionType;
  }

  public static CommentReaction From(UserId userId, ReactionType reactionType)
  {
    if (userId is null)
      throw new DomainValidationException("UserId is required for reaction.", nameof(userId));
    return new CommentReaction(userId, reactionType);
  }

#pragma warning disable CS8618
  private CommentReaction() { }
#pragma warning restore CS8618

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return UserId;
  }
}
