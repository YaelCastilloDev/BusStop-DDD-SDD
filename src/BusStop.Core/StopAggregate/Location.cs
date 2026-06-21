using Ardalis.SharedKernel;

namespace BusStop.Core.StopAggregate;

public sealed class Location(double latitude, double longitude) : ValueObject
{
  public double Latitude { get; } = Guard.Against.OutOfRange(latitude, nameof(latitude), -90, 90);
  public double Longitude { get; } = Guard.Against.OutOfRange(longitude, nameof(longitude), -180, 180);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Latitude;
    yield return Longitude;
  }
}
