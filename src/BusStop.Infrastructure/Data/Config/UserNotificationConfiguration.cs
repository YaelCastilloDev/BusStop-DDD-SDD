using BusStop.Core.NotificationAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusStop.Infrastructure.Data.Config;

public class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
  public void Configure(EntityTypeBuilder<UserNotification> builder)
  {
    builder.HasKey(x => x.Id);

    builder.Property(x => x.UserId)
           .HasConversion(x => x.Value, x => new Core.UserAggregate.UserId(x))
           .IsRequired();

    builder.Property(x => x.Title)
           .HasMaxLength(DataSchemaConstants.DEFAULT_TITLE_LENGTH)
           .IsRequired();

    builder.Property(x => x.Message)
           .HasMaxLength(DataSchemaConstants.DEFAULT_CONTENT_LENGTH)
           .IsRequired();

    builder.Property(x => x.IsRead)
           .IsRequired();

    builder.Property(x => x.CreatedAt)
           .IsRequired();
  }
}
