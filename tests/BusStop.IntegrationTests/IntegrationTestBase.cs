using BusStop.Infrastructure.Data;

namespace BusStop.IntegrationTests;

public abstract class IntegrationTestBase : IDisposable
{
    protected readonly AppDbContext DbContext;

    protected IntegrationTestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new AppDbContext(options);
    }

    public void Dispose()
    {
        DbContext.Dispose();
    }
}
