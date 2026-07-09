using Ardalis.SharedKernel;
using BusStop.Core.Exceptions;

namespace BusStop.Core.StopAggregate;

public sealed class Location(double latitude, double longitude) : ValueObject
{
  public double Latitude { get; } = latitude >= -90 && latitude <= 90
    ? latitude
    : throw new DomainValidationException("Latitude must be between -90 and 90.", nameof(latitude));

  public double Longitude { get; } = longitude >= -180 && longitude <= 180
    ? longitude
    : throw new DomainValidationException("Longitude must be between -180 and 180.", nameof(longitude));

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Latitude;
    yield return Longitude;
  }
}
