namespace BusStop.Core.Errors;

public static class StopErrors
{
    public const string EmptyName = "Stop name is required.";
    public const string InvalidLatitude = "Latitude must be between -90 and 90.";
    public const string InvalidLongitude = "Longitude must be between -180 and 180.";
    public const string InvalidRouteId = "Route ID must be positive.";
    public const string AlreadyDeleted = "Stop has already been deleted.";
}
