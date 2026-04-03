using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Infrastructure.Data.Configurations
{
	public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
	{
		public void Configure(EntityTypeBuilder<JobApplication> entity)
		{
			// Khóa chính
			entity.HasKey(e => e.Id);

			// Index cho các cột thường xuyên truy vấn
			entity.HasIndex(e => e.JobId)
				.HasDatabaseName("IX_JobApplications_JobId");

			entity.HasIndex(e => e.CVId)
				.HasDatabaseName("IX_JobApplications_CVId");

			entity.HasIndex(e => e.Status)
				.HasDatabaseName("IX_JobApplications_Status");

			entity.HasIndex(e => e.AppliedAt)
				.HasDatabaseName("IX_JobApplications_AppliedAt");

			// Composite index cho tìm kiếm theo Job và Status
			entity.HasIndex(e => new { e.JobId, e.Status })
				.HasDatabaseName("IX_JobApplications_JobId_Status");

			// Composite index cho tìm kiếm theo CV và Status
			entity.HasIndex(e => new { e.CVId, e.Status })
				.HasDatabaseName("IX_JobApplications_CVId_Status");

			// Cấu hình các thuộc tính
			entity.Property(e => e.Id)
				.HasDefaultValueSql("NEWID()");

			entity.Property(e => e.JobId)
				.IsRequired();

			entity.Property(e => e.CVId)
				.IsRequired();

			entity.Property(e => e.Status)
				.IsRequired()
				.HasConversion<int>()
				.HasDefaultValue(JobApplicationStatus.Pending);

			entity.Property(e => e.AppliedAt)
				.IsRequired()
				.HasDefaultValueSql("GETUTCDATE()");

			entity.Property(e => e.ReviewedAt)
				.IsRequired(false);

			entity.Property(e => e.Notes)
				.HasMaxLength(2000)
				.IsRequired(false);

			// Quan hệ với Job
			entity.HasOne(e => e.Job)
				.WithMany()
				.HasForeignKey(e => e.JobId)
				.OnDelete(DeleteBehavior.Restrict) // Không cho xóa Job nếu có ứng viên
				.HasConstraintName("FK_JobApplications_Job");

			// Quan hệ với CV
			entity.HasOne(e => e.CV)
				.WithMany()
				.HasForeignKey(e => e.CVId)
				.OnDelete(DeleteBehavior.Restrict) // Không cho xóa CV nếu có ứng tuyển
				.HasConstraintName("FK_JobApplications_CV");

			// Quan hệ 1-1 với JobApplicationMatch
			entity.HasOne(e => e.Match)
				.WithOne(m => m.Application)
				.HasForeignKey<JobApplicationMatch>(m => m.ApplicationId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}