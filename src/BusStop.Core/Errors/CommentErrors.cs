namespace BusStop.Core.Errors;

public static class CommentErrors
{
    public const string EmptyContent = "Comment content is required.";
    public const string InvalidUser = "User ID must be valid.";
    public const string InvalidRoute = "Route ID must be valid.";
    public const string AlreadyModerated = "Comment has already been moderated.";
}
