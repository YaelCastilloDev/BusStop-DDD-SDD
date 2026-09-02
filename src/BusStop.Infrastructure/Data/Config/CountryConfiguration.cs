using BusStop.Core.CountryAggregate;

namespace BusStop.Infrastructure.Data.Config;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
  public void Configure(EntityTypeBuilder<Country> builder)
  {
    builder.ToTable("countries");

    builder.HasKey(c => c.Id);
    builder.Property(c => c.Id).ValueGeneratedOnAdd();

    builder.Property(c => c.Name)
           .HasMaxLength(DataSchemaConstants.DEFAULT_NAME_LENGTH)
           .IsRequired();

    builder.HasIndex(c => c.Name).IsUnique();

    builder.Property(c => c.IsoCode)
           .HasMaxLength(2)
           .IsRequired();

    builder.HasIndex(c => c.IsoCode).IsUnique();
  }
}
