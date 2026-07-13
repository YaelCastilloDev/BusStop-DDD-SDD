using BusStop.Core.ModerationActionAggregate;
using BusStop.Core.UserAggregate;

namespace BusStop.Infrastructure.Data.Config;

public sealed class ModerationActionConfiguration : IEntityTypeConfiguration<ModerationAction>
{
    public void Configure(EntityTypeBuilder<ModerationAction> builder)
    {
        builder.ToTable("moderation_actions");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedOnAdd();

        // Foreign key constraints to target_types and moderation_categories lookup tables
        // are enforced at the DB level via migration raw SQL, not in the EF model.
        builder.Property(m => m.TargetType)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(m => m.TargetId).IsRequired();

        builder.Property(m => m.UserId)
               .HasConversion(id => id.Value, value => new UserId(value))
               .IsRequired();

        builder.Property(m => m.IssuedBy)
               .HasConversion(id => id.Value, value => new UserId(value))
               .IsRequired();

        builder.Property(m => m.Category)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(m => m.Reason)
               .HasConversion(reason => reason.Value, value => Reason.From(value))
               .HasMaxLength(DataSchemaConstants.DEFAULT_CONTENT_LENGTH)
               .IsRequired();

        builder.Property(m => m.IssuedAt).IsRequired();

        builder.HasIndex(m => m.UserId);
        builder.HasIndex(m => m.IssuedBy);
        builder.HasIndex(m => new { m.TargetType, m.TargetId });
    }
}
