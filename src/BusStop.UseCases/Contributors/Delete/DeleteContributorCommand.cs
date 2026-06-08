using BusStop.Core.ContributorAggregate;

namespace BusStop.UseCases.Contributors.Delete;

public record DeleteContributorCommand(ContributorId ContributorId) : ICommand<Result>;
