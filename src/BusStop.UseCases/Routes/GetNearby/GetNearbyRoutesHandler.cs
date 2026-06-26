using BusStop.Core.RouteAggregate;

namespace BusStop.UseCases.Routes.GetNearby;

public sealed class GetNearbyRoutesHandler(INearbyRoutesQueryService queryService) : IQueryHandler<GetNearbyRoutesQuery, Result<NearbyRoutesResult>>
{
  public async ValueTask<Result<NearbyRoutesResult>> Handle(GetNearbyRoutesQuery request, CancellationToken cancellationToken)
  {
    var result = await queryService.GetNearbyRoutesAsync(
        request.Latitude, 
        request.Longitude, 
        request.InitialRadiusKm, 
        request.FallbackRadiusKm, 
        cancellationToken);

    return Result<NearbyRoutesResult>.Success(result);
  }
}

