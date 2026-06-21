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
  public DateTime? DeletedAt { get; private set; }
  public long? DeletedBy { get; private set; }

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

  public static Result<Comment> Create(string content, long userId, long routeId)
  {
    if (string.IsNullOrWhiteSpace(content))
      return Result<Comment>.Error("Comment content is required.");
    if (userId <= 0)
      return Result<Comment>.Error("User ID is required.");
    if (routeId <= 0)
      return Result<Comment>.Error("Route ID is required.");

    return Result<Comment>.Success(new Comment(new CommentContent(content), new UserId(userId), new RouteId(routeId)));
  }

  public void Delete(UserId deletedBy)
  {
    Guard.Against.Null(deletedBy);
    DeletedAt = DateTime.UtcNow;
    DeletedBy = deletedBy.Value;
    RegisterDomainEvent(new CommentDeletedEvent(Id));
  }

  public bool IsDeleted => DeletedAt.HasValue;

  public void AddReaction(UserId userId, ReactionType reactionType)
  {
    Guard.Against.Null(userId);
    _reactions.RemoveAll(r => r.UserId == userId);
    _reactions.Add(new CommentReaction(userId, reactionType));
  }
}
