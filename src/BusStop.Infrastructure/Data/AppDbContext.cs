using BusStop.Core.RouteAggregate;
using BusStop.Core.StopAggregate;
using BusStop.Core.UserAggregate;
using BusStop.Core.CommentAggregate;
using BusStop.Core.CountryAggregate;

namespace BusStop.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Route> Routes => Set<Route>();
  public DbSet<Stop> Stops => Set<Stop>();
  public DbSet<User> Users => Set<User>();
  public DbSet<Comment> Comments => Set<Comment>();
  public DbSet<Country> Countries => Set<Country>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }

  public override int SaveChanges() =>
        SaveChangesAsync().GetAwaiter().GetResult();
}
