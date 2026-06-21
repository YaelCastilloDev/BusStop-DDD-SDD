namespace BusStop.Infrastructure.Data;

public static class SeedData
{
  public static async Task InitializeAsync(AppDbContext dbContext)
  {
    // Seed data will be added as BusStop aggregates are implemented.
    await Task.CompletedTask;
  }
}
