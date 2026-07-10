using BusStop.Core.CommentAggregate.Events;
using BusStop.Core.Errors;
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

    private readonly List<CommentReaction> _reactions = [];
    public IReadOnlyCollection<CommentReaction> Reactions => _reactions.AsReadOnly();

#pragma warning disable CS8618
    private Comment() { }
#pragma warning restore CS8618

    private Comment(CommentContent content, UserId userId, RouteId routeId)
    {
        Guard.Against.Null(content, nameof(content));
        Guard.Against.Null(userId, nameof(userId));
        Guard.Against.Null(routeId, nameof(routeId));

        Content = content;
        UserId = userId;
        RouteId = routeId;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Comment> Create(string content, long userId, long routeId)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(content))
            errors.Add(CommentErrors.EmptyContent);
        if (userId <= 0)
            errors.Add(CommentErrors.InvalidUser);
        if (routeId <= 0)
            errors.Add(CommentErrors.InvalidRoute);

        if (errors.Count > 0)
            return Result<Comment>.Error(new ErrorList(errors));

        return Result<Comment>.Success(new Comment(new CommentContent(content), new UserId(userId), new RouteId(routeId)));
    }

    public Result Moderate(UserId moderatedBy)
    {
        Guard.Against.Null(moderatedBy, nameof(moderatedBy));

        if (IsModerated)
            return Result.Error(new ErrorList([CommentErrors.AlreadyModerated]));

        ModeratedAt = DateTime.UtcNow;
        ModeratedBy = moderatedBy.Value;
        RegisterDomainEvent(new CommentModeratedEvent(Id, UserId.Value));
        return Result.Success();
    }

    public bool IsModerated => ModeratedAt.HasValue;

    public void AddReaction(UserId userId, ReactionType reactionType)
    {
        Guard.Against.Null(userId, nameof(userId));
        _reactions.RemoveAll(r => r.UserId == userId);
        _reactions.Add(CommentReaction.From(userId, reactionType));
    }
}
