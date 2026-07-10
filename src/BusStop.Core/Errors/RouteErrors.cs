namespace BusStop.Core.Errors;

public static class RouteErrors
{
    public const string EmptyName = "Route name is required.";
    public const string InvalidCreatedBy = "CreatedBy user ID must be valid.";
    public const string AlreadyDeleted = "Route has already been deleted.";
}
