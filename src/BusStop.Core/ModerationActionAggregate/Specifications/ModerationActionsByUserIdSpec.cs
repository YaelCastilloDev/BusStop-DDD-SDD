namespace BusStop.Core.ModerationActionAggregate.Specifications;

public sealed class ModerationActionsByUserIdSpec : Specification<ModerationAction>
{
    public ModerationActionsByUserIdSpec(long userId) =>
        Query.Where(m => m.UserId.Value == userId);
}
