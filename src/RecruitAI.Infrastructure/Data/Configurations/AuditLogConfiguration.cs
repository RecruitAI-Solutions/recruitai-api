using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Infrastructure.Data.Configurations
{
	public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
	{
		public void Configure(EntityTypeBuilder<AuditLog> entity)
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.EntityType)
				.IsRequired()
				.HasConversion<int>();  // Lưu dưới dạng int

			entity.Property(e => e.Action)
				.IsRequired()
				.HasConversion<int>();  // Lưu dưới dạng int

			entity.Property(e => e.EntityId)
				.IsRequired()
				.HasMaxLength(50);

			entity.Property(e => e.EntityName)
				.IsRequired()
				.HasMaxLength(256);

			entity.Property(e => e.Reason)
				.HasMaxLength(500)
				.IsRequired(false);

			entity.Property(e => e.ChangedBy)
				.IsRequired()
				.HasMaxLength(256);

			entity.Property(e => e.ChangedByIp)
				.HasMaxLength(45)
				.IsRequired(false);

			entity.Property(e => e.UserAgent)
				.HasMaxLength(500)
				.IsRequired(false);

			entity.Property(e => e.RequestId)
				.HasMaxLength(100)
				.IsRequired(false);

			entity.Property(e => e.ChangedAt)
				.HasDefaultValueSql("GETUTCDATE()");

			// Indexes
			entity.HasIndex(e => new { e.EntityType, e.EntityId })
				.HasDatabaseName("IX_AuditLogs_EntityType_EntityId");

			entity.HasIndex(e => e.ChangedAt)
				.HasDatabaseName("IX_AuditLogs_ChangedAt");

			entity.HasIndex(e => e.Action)
				.HasDatabaseName("IX_AuditLogs_Action");

			entity.HasIndex(e => e.ChangedBy)
				.HasDatabaseName("IX_AuditLogs_ChangedBy");
		}
	}
}