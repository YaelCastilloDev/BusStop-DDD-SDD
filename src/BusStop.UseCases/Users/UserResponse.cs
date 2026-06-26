namespace BusStop.UseCases.Users;

public sealed record UserResponse(long Id, string? Username, string Email, string? ExternalId, DateTime CreatedAt, long? CountryId);
