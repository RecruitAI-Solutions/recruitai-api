using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Infrastructure.Data.Configurations;

public class SavedJobConfiguration : IEntityTypeConfiguration<SavedJob>
{
	public void Configure(EntityTypeBuilder<SavedJob> builder)
	{
		builder.ToTable("SavedJobs");

		builder.HasKey(s => s.Id);

		builder.Property(s => s.SavedAt)
			.IsRequired()
			.HasDefaultValueSql("GETUTCDATE()");

		// Unique constraint: một user chỉ lưu một job một lần
		builder.HasIndex(s => new { s.JobId, s.UserId })
			.IsUnique()
			.HasDatabaseName("IX_SavedJobs_JobId_UserId");

		// Relationship với Job
		builder.HasOne(s => s.Job)
			.WithMany()
			.HasForeignKey(s => s.JobId)
			.OnDelete(DeleteBehavior.Cascade);

		// Relationship với User
		builder.HasOne(s => s.User)
			.WithMany()
			.HasForeignKey(s => s.UserId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}