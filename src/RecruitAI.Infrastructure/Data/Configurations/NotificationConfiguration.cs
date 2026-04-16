using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Infrastructure.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
	public void Configure(EntityTypeBuilder<Notification> builder)
	{
		builder.ToTable("Notifications");

		builder.HasKey(n => n.Id);

		builder.Property(n => n.Title)
			.IsRequired()
			.HasMaxLength(255);

		builder.Property(n => n.Content)
			.IsRequired()
			.HasMaxLength(2000);

		builder.Property(n => n.Type)
			.IsRequired()
			.HasMaxLength(50);

		builder.Property(n => n.IsRead)
			.HasDefaultValue(false);

		builder.Property(n => n.Data)
			.HasMaxLength(2000);

		builder.Property(n => n.CreatedAt)
			.IsRequired()
			.HasDefaultValueSql("GETUTCDATE()");

		builder.HasIndex(n => n.UserId)
			.HasDatabaseName("IX_Notifications_UserId");

		builder.HasIndex(n => n.IsRead)
			.HasDatabaseName("IX_Notifications_IsRead");

		builder.HasIndex(n => n.CreatedAt)
			.HasDatabaseName("IX_Notifications_CreatedAt");

		builder.HasOne(n => n.User)
			.WithMany()
			.HasForeignKey(n => n.UserId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}