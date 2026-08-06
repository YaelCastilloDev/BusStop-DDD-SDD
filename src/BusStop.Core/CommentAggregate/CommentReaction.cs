using Ardalis.SharedKernel;
using BusStop.Core.UserAggregate;

namespace BusStop.Core.CommentAggregate;

public sealed class CommentReaction : ValueObject
{
    public UserId UserId { get; }
    public bool IsLike { get; }

    private CommentReaction(UserId userId, bool isLike)
    {
        UserId = userId;
        IsLike = isLike;
    }

    public static CommentReaction From(UserId userId, bool isLike)
    {
        Guard.Against.Null(userId, nameof(userId));
        return new CommentReaction(userId, isLike);
    }

#pragma warning disable CS8618
    private CommentReaction() { }
#pragma warning restore CS8618

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return UserId;
        yield return IsLike;
    }
}
