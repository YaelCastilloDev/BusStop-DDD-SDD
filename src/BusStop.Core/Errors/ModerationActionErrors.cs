namespace BusStop.Core.Errors;

public static class ModerationActionErrors
{
    public const string InvalidCategory = "Moderation category is invalid.";
    public const string InvalidTargetType = "Target type must be Comment (1) or Route (2).";
    public const string InvalidTargetId = "Target ID must be valid.";
    public const string InvalidUserId = "User ID must be valid.";
    public const string InvalidIssuedBy = "IssuedBy user ID must be valid.";
    public const string EmptyReason = "Moderation reason is required.";
}
