using BusStop.Core.ContributorAggregate;

namespace BusStop.UseCases.Contributors.Get;

public record GetContributorQuery(ContributorId ContributorId) : IQuery<Result<ContributorDto>>;
