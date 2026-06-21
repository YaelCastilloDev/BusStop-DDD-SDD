namespace BusStop.UseCases.Routes;

public sealed record RouteResponse(long Id, string Name, long CreatedById, DateTime CreatedAt, bool IsDeleted);
