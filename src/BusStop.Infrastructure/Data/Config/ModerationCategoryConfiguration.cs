namespace BusStop.Infrastructure.Data.Config;

public sealed class ModerationCategoryConfiguration : IEntityTypeConfiguration<ModerationCategoryLookup>
{
    public void Configure(EntityTypeBuilder<ModerationCategoryLookup> builder)
    {
        builder.ToTable("moderation_categories");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(m => m.Name).IsUnique();
    }
}
