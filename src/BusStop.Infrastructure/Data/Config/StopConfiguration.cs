using BusStop.Core.StopAggregate;
using BusStop.Core.RouteAggregate;
using BusStop.Core.UserAggregate;

namespace BusStop.Infrastructure.Data.Config;

public sealed class StopConfiguration : IEntityTypeConfiguration<Stop>
{
  public void Configure(EntityTypeBuilder<Stop> builder)
  {
    builder.ToTable("stops");

    builder.HasKey(s => s.Id);
    builder.Property(s => s.Id).ValueGeneratedNever();

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
           .HasConversion(id => id.Value, value => new RouteId(value))
           .IsRequired();

    builder.Property(s => s.DeletedAt).IsRequired(false);
    builder.Property(s => s.DeletedBy).IsRequired(false);

    builder.HasQueryFilter(s => s.DeletedAt == null);
  }
}
