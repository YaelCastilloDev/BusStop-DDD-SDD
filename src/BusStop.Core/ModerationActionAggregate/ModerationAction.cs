using BusStop.Core.Errors;
using BusStop.Core.UserAggregate;

namespace BusStop.Core.ModerationActionAggregate;

public class ModerationAction : EntityBase<long>, IAggregateRoot
{
    public TargetType TargetType { get; private set; }
    public long TargetId { get; private set; }
    public UserId UserId { get; private set; }
    public UserId IssuedBy { get; private set; }
    public ModerationCategory Category { get; private set; }
    public Reason Reason { get; private set; }
    public DateTime IssuedAt { get; private set; }

#pragma warning disable CS8618
    private ModerationAction() { }
#pragma warning restore CS8618

    private ModerationAction(TargetType targetType, long targetId, UserId userId, UserId issuedBy, ModerationCategory category, Reason reason)
    {
        Guard.Against.Null(userId, nameof(userId));
        Guard.Against.Null(issuedBy, nameof(issuedBy));
        Guard.Against.Null(reason, nameof(reason));

        TargetType = targetType;
        TargetId = targetId;
        UserId = userId;
        IssuedBy = issuedBy;
        Category = category;
        Reason = reason;
        IssuedAt = DateTime.UtcNow;
    }

    public static Result<ModerationAction> Create(TargetType targetType, long targetId, long userId, long issuedBy, ModerationCategory category, string reason)
    {
        var errors = new List<string>();

        if (!Enum.IsDefined(typeof(TargetType), targetType))
            errors.Add(ModerationActionErrors.InvalidTargetType);
        if (targetId <= 0)
            errors.Add(ModerationActionErrors.InvalidTargetId);
        if (userId <= 0)
            errors.Add(ModerationActionErrors.InvalidUserId);
        if (issuedBy <= 0)
            errors.Add(ModerationActionErrors.InvalidIssuedBy);
        if (!Enum.IsDefined(typeof(ModerationCategory), category))
            errors.Add(ModerationActionErrors.InvalidCategory);
        if (string.IsNullOrWhiteSpace(reason))
            errors.Add(ModerationActionErrors.EmptyReason);

        if (errors.Count > 0)
            return Result<ModerationAction>.Error(new ErrorList(errors));

        var action = new ModerationAction(targetType, targetId, new UserId(userId), new UserId(issuedBy), category, Reason.From(reason));
        return Result<ModerationAction>.Success(action);
    }

}
