using Ardalis.Result;

namespace BusStop.Core.Interfaces;

public interface IKeycloakAdminService
{
    Task<Result> CreateUserAsync(string email, string password, CancellationToken ct);
}
