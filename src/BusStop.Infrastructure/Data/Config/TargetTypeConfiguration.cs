namespace BusStop.Infrastructure.Data.Config;

public sealed class TargetTypeConfiguration : IEntityTypeConfiguration<TargetTypeLookup>
{
    public void Configure(EntityTypeBuilder<TargetTypeLookup> builder)
    {
        builder.ToTable("target_types");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(t => t.Name).IsUnique();
    }
}
