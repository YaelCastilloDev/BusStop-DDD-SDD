using BusStop.Core.CommentAggregate.Events;
using BusStop.Core.RouteAggregate;
using BusStop.Core.UserAggregate;

namespace BusStop.Core.CommentAggregate;

public class Comment : EntityBase<long>, IAggregateRoot
{
  public CommentContent Content { get; private set; }
  public UserId UserId { get; private set; }
  public RouteId RouteId { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime? ModeratedAt { get; private set; }
  public long? ModeratedBy { get; private set; }

  // We store reactions as a JSON column (Value Objects) rather than a separate table.
  // This is performant and aligns with DDD since a reaction has no identity outside its comment.
  // NOTE: This approach is ideal for low-to-moderate reaction counts (e.g., < 1000 per comment).
  // If reactions scale to massive numbers (e.g., 10,000+), EF Core rewriting the entire JSON 
  // string on every update will become a bottleneck, and this should be moved to a separate table.
  private readonly List<CommentReaction> _reactions = [];
  public IReadOnlyCollection<CommentReaction> Reactions => _reactions.AsReadOnly();

#pragma warning disable CS8618
  private Comment() { }
#pragma warning restore CS8618

  private Comment(CommentContent content, UserId userId, RouteId routeId)
  {
    Content = content;
    UserId = userId;
    RouteId = routeId;
    CreatedAt = DateTime.UtcNow;
  }

  public static Comment Create(string content, long userId, long routeId)
  {
    Guard.Against.NullOrWhiteSpace(content);
    Guard.Against.NegativeOrZero(userId);
    Guard.Against.NegativeOrZero(routeId);

    return new Comment(new CommentContent(content), new UserId(userId), new RouteId(routeId));
  }

  // Moderate is for administrative takedowns. A future Remove method could be added for user-driven deletions.
  public void Moderate(UserId moderatedBy)
  {
    Guard.Against.Null(moderatedBy);
    ModeratedAt = DateTime.UtcNow;
    ModeratedBy = moderatedBy.Value;
    RegisterDomainEvent(new CommentModeratedEvent(Id));
  }

  public bool IsModerated => ModeratedAt.HasValue;

  public void AddReaction(UserId userId, ReactionType reactionType)
  {
    Guard.Against.Null(userId);
    _reactions.RemoveAll(r => r.UserId == userId);
    _reactions.Add(CommentReaction.From(userId, reactionType));
  }
}
