namespace BusStop.UseCases.Routes.List;

public sealed record ListRoutesQuery(int Page = 1, int PageSize = 20) : IQuery<Result<List<RouteResponse>>>;
