using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Infrastructure.Data.Configurations
{
	public class JobConfiguration : IEntityTypeConfiguration<Job>
	{
		public void Configure(EntityTypeBuilder<Job> entity)
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Title)
				.IsRequired()
				.HasMaxLength(255);

			entity.Property(e => e.Description)
				.IsRequired()
				.HasMaxLength(4000);

			entity.Property(e => e.Requirements)
				.HasMaxLength(4000);

			entity.Property(e => e.Location)
				.IsRequired()
				.HasMaxLength(255);

			entity.Property(e => e.SalaryMin)
				.HasPrecision(18, 2)
				.IsRequired(false);

			entity.Property(e => e.SalaryMax)
				.HasPrecision(18, 2)
				.IsRequired(false);

			entity.Property(e => e.Currency)
				.HasConversion<int>()
				.HasDefaultValue(Currency.VND);

			entity.Property(e => e.EmploymentType)
				.HasConversion<int>();

			entity.Property(e => e.ExperienceLevel)
				.HasConversion<int>();

			entity.Property(e => e.Status)
				.HasConversion<int>()
				.HasDefaultValue(JobStatus.Draft);

			entity.Property(e => e.Skills)
				.HasMaxLength(500);

			entity.Property(e => e.Benefits)
				.HasMaxLength(2000);

			entity.Property(e => e.CreatedAt)
				.HasDefaultValueSql("GETUTCDATE()");

			entity.Property(e => e.IsDeleted)
				.HasDefaultValue(false);

			entity.HasOne(e => e.Recruiter)
				.WithMany()
				.HasForeignKey(e => e.RecruiterId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasIndex(e => e.RecruiterId).HasDatabaseName("IX_Jobs_RecruiterId");
			entity.HasIndex(e => e.Title).HasDatabaseName("IX_Jobs_Title");
			entity.HasIndex(e => e.Location).HasDatabaseName("IX_Jobs_Location");
			entity.HasIndex(e => e.EmploymentType).HasDatabaseName("IX_Jobs_EmploymentType");
			entity.HasIndex(e => e.ExperienceLevel).HasDatabaseName("IX_Jobs_ExperienceLevel");
			entity.HasIndex(e => e.Status).HasDatabaseName("IX_Jobs_Status");
			entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_Jobs_CreatedAt");
			entity.HasIndex(e => e.ExpirationDate).HasDatabaseName("IX_Jobs_ExpirationDate");
			entity.HasIndex(e => e.IsDeleted).HasDatabaseName("IX_Jobs_IsDeleted");
		}
	}
}