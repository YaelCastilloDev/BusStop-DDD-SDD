namespace BusStop.Core.ModerationActionAggregate.Specifications;

public sealed class ModerationActionByIdSpec : Specification<ModerationAction>
{
    public ModerationActionByIdSpec(ModerationActionId moderationActionId) =>
        Query.Where(m => m.Id == moderationActionId.Value);
}
