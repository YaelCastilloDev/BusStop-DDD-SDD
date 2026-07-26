using Npgsql;

namespace BusStop.IntegrationTests.Data;

[Collection("PostgreSQL")]
public class PgStatStatementsTests : IntegrationTestBase
{
    public PgStatStatementsTests(PostgreSqlFixture fixture) : base(fixture) { }

    [Fact]
    public async Task PgStatStatements_IsEnabled_AndTracksQueries()
    {
        await using var conn = new NpgsqlConnection(DbContext.Database.GetConnectionString());
        await conn.OpenAsync(Current.CancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1";
        await cmd.ExecuteNonQueryAsync(Current.CancellationToken);

        cmd.CommandText = "SELECT count(*) AS query_count FROM pg_stat_statements";
        await using var reader = await cmd.ExecuteReaderAsync(Current.CancellationToken);
        var hasRows = await reader.ReadAsync(Current.CancellationToken);
        hasRows.ShouldBeTrue();
        var count = reader.GetInt64(0);
        count.ShouldBeGreaterThan(0L);
    }

    [Fact]
    public async Task PgStatStatements_ContainsExecutedQuery()
    {
        await using var conn = new NpgsqlConnection(DbContext.Database.GetConnectionString());
        await conn.OpenAsync(Current.CancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 'test-marker-' || gen_random_uuid()::text AS marker";
        await cmd.ExecuteNonQueryAsync(Current.CancellationToken);

        await Task.Delay(100, Current.CancellationToken);

        cmd.CommandText = "SELECT count(*) FROM pg_stat_statements WHERE query LIKE '%test-marker-%'";
        var count = (long)(await cmd.ExecuteScalarAsync(Current.CancellationToken))!;
        count.ShouldBeGreaterThan(0L);
    }
}
