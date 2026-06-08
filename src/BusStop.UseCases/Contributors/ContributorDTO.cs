using BusStop.Core.ContributorAggregate;

namespace BusStop.UseCases.Contributors;
public record ContributorDto(ContributorId Id, ContributorName Name, PhoneNumber PhoneNumber);
