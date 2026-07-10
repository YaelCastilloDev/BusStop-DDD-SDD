using Ardalis.SharedKernel;

namespace BusStop.Core.StopAggregate;

public sealed class Location : ValueObject
{
    public double Latitude { get; }
    public double Longitude { get; }

    public Location(double latitude, double longitude)
    {
        Guard.Against.OutOfRange(latitude, nameof(latitude), -90, 90, "Latitude must be between -90 and 90.");
        Guard.Against.OutOfRange(longitude, nameof(longitude), -180, 180, "Longitude must be between -180 and 180.");
        Latitude = latitude;
        Longitude = longitude;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }
}
