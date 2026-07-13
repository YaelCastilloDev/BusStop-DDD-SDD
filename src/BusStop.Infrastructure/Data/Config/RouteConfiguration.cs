using BusStop.Core.RouteAggregate;
using BusStop.Core.UserAggregate;

namespace BusStop.Infrastructure.Data.Config;

public sealed class RouteConfiguration : IEntityTypeConfiguration<Route>
{
  public void Configure(EntityTypeBuilder<Route> builder)
  {
    builder.ToTable("routes");

    builder.HasKey(r => r.Id);
    builder.Property(r => r.Id).ValueGeneratedOnAdd();

    builder.Property(r => r.Name)
           .HasConversion(name => name.Value, value => new RouteName(value))
           .HasMaxLength(DataSchemaConstants.DEFAULT_NAME_LENGTH)
           .IsRequired();

    builder.Property(r => r.CreatedById)
           .HasConversion(id => id.Value, value => new UserId(value))
           .IsRequired();

    builder.Property(r => r.CreatedAt).IsRequired();
    builder.Property(r => r.DeletedAt).IsRequired(false);
    builder.Property(r => r.DeletedBy).IsRequired(false);
    builder.Property(r => r.ModeratedAt).IsRequired(false);
    builder.Property(r => r.ModeratedBy).IsRequired(false);

    builder.HasQueryFilter(r => r.DeletedAt == null && r.ModeratedAt == null);
  }
}
