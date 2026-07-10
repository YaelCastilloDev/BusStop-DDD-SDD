using BusStop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BusStop.IntegrationTests;

[Collection("PostgreSQL")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private AppDbContext? _dbContext;
    private readonly string _databaseName;
    private string? _dbConnectionString;

    protected AppDbContext DbContext => _dbContext!;

    protected IntegrationTestBase(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
        _databaseName = $"itest_{Guid.NewGuid():N}";
    }

    public virtual async ValueTask InitializeAsync()
    {
        var masterConnString = _fixture.GetConnectionString();
        _dbConnectionString = masterConnString.Replace("Database=busstop_test", $"Database={_databaseName}");

        await using var masterConn = new NpgsqlConnection(masterConnString);
        await masterConn.OpenAsync();
        await using var cmd = masterConn.CreateCommand();
        cmd.CommandText = $"CREATE DATABASE \"{_databaseName}\"";
        await cmd.ExecuteNonQueryAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_dbConnectionString, npgsql => npgsql.UseNetTopologySuite())
            .Options;

        _dbContext = new AppDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (_dbContext is not null)
            await _dbContext.DisposeAsync();

        var masterConnString = _fixture.GetConnectionString();
        await using var masterConn = new NpgsqlConnection(masterConnString);
        await masterConn.OpenAsync();

        await using var cmd = masterConn.CreateCommand();
        cmd.CommandText = $"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)";
        await cmd.ExecuteNonQueryAsync();
    }
}
