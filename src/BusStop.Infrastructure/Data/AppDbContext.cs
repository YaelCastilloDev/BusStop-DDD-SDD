using BusStop.Core.CommentAggregate;
using BusStop.Core.CountryAggregate;
using BusStop.Core.ModerationActionAggregate;
using BusStop.Core.NotificationAggregate;
using BusStop.Core.RouteAggregate;
using BusStop.Core.StopAggregate;
using BusStop.Core.UserAggregate;
using BusStop.Infrastructure.Data.Config;

namespace BusStop.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Route> Routes => Set<Route>();
  public DbSet<Stop> Stops => Set<Stop>();
  public DbSet<User> Users => Set<User>();
  public DbSet<Comment> Comments => Set<Comment>();
  public DbSet<Country> Countries => Set<Country>();
  public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
  public DbSet<ModerationAction> ModerationActions => Set<ModerationAction>();
  public DbSet<TargetTypeLookup> TargetTypes => Set<TargetTypeLookup>();
  public DbSet<ModerationCategoryLookup> ModerationCategories => Set<ModerationCategoryLookup>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }

  public override int SaveChanges() =>
    throw new NotSupportedException("Use SaveChangesAsync instead.");
}
