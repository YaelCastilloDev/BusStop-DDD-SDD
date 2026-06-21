using BusStop.Core.CommentAggregate;
using BusStop.Core.RouteAggregate;
using BusStop.Core.UserAggregate;

namespace BusStop.Infrastructure.Data.Config;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
  public void Configure(EntityTypeBuilder<Comment> builder)
  {
    builder.ToTable("comments");

    builder.HasKey(c => c.Id);
    builder.Property(c => c.Id).ValueGeneratedNever();

    builder.Property(c => c.Content)
           .HasConversion(content => content.Value, value => new CommentContent(value))
           .HasMaxLength(DataSchemaConstants.DEFAULT_CONTENT_LENGTH)
           .IsRequired();

    builder.Property(c => c.UserId)
           .HasConversion(id => id.Value, value => new UserId(value))
           .IsRequired();

    builder.Property(c => c.RouteId)
           .HasConversion(id => id.Value, value => new RouteId(value))
           .IsRequired();

    builder.Property(c => c.CreatedAt).IsRequired();
    builder.Property(c => c.DeletedAt).IsRequired(false);
    builder.Property(c => c.DeletedBy).IsRequired(false);

    builder.OwnsMany(c => c.Reactions, r =>
    {
      r.ToJson("reactions");
      r.Property(x => x.UserId)
       .HasConversion(id => id.Value, value => new UserId(value));
      r.Property(x => x.ReactionType)
       .HasConversion<int>();
    });

    builder.HasQueryFilter(c => c.DeletedAt == null);
  }
}
