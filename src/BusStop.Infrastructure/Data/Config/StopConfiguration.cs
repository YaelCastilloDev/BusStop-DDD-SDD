using BusStop.Core.RouteAggregate;
using BusStop.Core.StopAggregate;
using BusStop.Core.UserAggregate;

namespace BusStop.Infrastructure.Data.Config;

public sealed class StopConfiguration : IEntityTypeConfiguration<Stop>
{
  public void Configure(EntityTypeBuilder<Stop> builder)
  {
    builder.ToTable("stops");

    builder.HasKey(s => s.Id);
    builder.Property(s => s.Id).ValueGeneratedOnAdd();

    builder.Property(s => s.Name)
           .HasConversion(name => name.Value, value => new StopName(value))
           .HasMaxLength(DataSchemaConstants.DEFAULT_NAME_LENGTH)
           .IsRequired();

    builder.Property(s => s.Location)
           .HasConversion(
             location => new NetTopologySuite.Geometries.Point(location.Longitude, location.Latitude) { SRID = 4326 },
             point => new Location(point.Y, point.X))
           .HasColumnType("geography (point)");

    builder.Property(s => s.RouteId)
           .IsRequired();

    builder.Property(s => s.DeletedAt).IsRequired(false);
    builder.Property(s => s.DeletedBy).IsRequired(false);

    builder.HasIndex(s => s.RouteId);

    builder.HasOne<Route>()
           .WithMany()
           .HasForeignKey(s => s.RouteId)
           .HasConstraintName("fk_stops_route")
           .OnDelete(DeleteBehavior.Cascade);

    builder.HasQueryFilter(s => s.DeletedAt == null);
  }
}
