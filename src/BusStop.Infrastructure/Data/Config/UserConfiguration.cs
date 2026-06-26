using BusStop.Core.UserAggregate;

namespace BusStop.Infrastructure.Data.Config;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.ToTable("users");

    builder.HasKey(u => u.Id);
    builder.Property(u => u.Id).ValueGeneratedOnAdd();

    builder.Property(u => u.Username)
           .HasConversion(name => name!.Value, value => new Username(value))
           .HasMaxLength(50)
           .IsRequired(false);

    builder.HasIndex(u => u.Username).IsUnique();

    builder.Property(u => u.Email)
           .HasMaxLength(DataSchemaConstants.DEFAULT_EMAIL_LENGTH)
           .IsRequired();

    builder.Property(u => u.ExternalId)
           .HasMaxLength(64)
           .IsRequired(false);

    builder.HasIndex(u => u.ExternalId).IsUnique();

    builder.Property(u => u.CountryId)
           .IsRequired(false);

    builder.HasIndex(u => u.CountryId);

    builder.Property(u => u.CreatedAt).IsRequired();
  }
}
