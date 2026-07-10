using Testcontainers.PostgreSql;

namespace BusStop.IntegrationTests;

// TODO: Deferred — username-uniqueness domain invariant not enforced in OnboardingHandler
// (no duplicate-username check). Keycloak handles email uniqueness, not BusStop usernames.
// Add a UserByUsernameSpec and guard in OnboardingHandler before closing SPEC-IdentityAccess-RegisterFlow.
//
// TODO: Deferred — 16 untested use-case handlers (ListRoutes, CreateRoute, GetRouteById,
// DeleteRoute, CreateStop, GetStopsByRoute, CreateComment, GetCommentsByRoute, ReactToComment,
// ModerateComment, ListCountries, GetMeHandler, GetUserByIdHandler, GetMyNotifications,
// DeleteNotification, ProcessModerationNotification). Add unit/integration tests incrementally.
//
// TODO: Deferred — CountryAggregate and NotificationAggregate are missing Events/ and Handlers/
// subfolders. Currently no domain events exist for these aggregates, so the folders are not yet
// needed but should be created when events are introduced.

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Container = new PostgreSqlBuilder("postgis/postgis:15-3.3")
            .WithDatabase("busstop_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await Container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (Container is not null)
            await Container.DisposeAsync();
    }

    public string GetConnectionString() => Container.GetConnectionString();
}

[CollectionDefinition("PostgreSQL")]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
}
