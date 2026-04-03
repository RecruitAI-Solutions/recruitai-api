using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Infrastructure.Data.Configurations
{
	public class JobApplicationMatchConfiguration : IEntityTypeConfiguration<JobApplicationMatch>
	{
		public void Configure(EntityTypeBuilder<JobApplicationMatch> entity)
		{
			// Khóa chính
			entity.HasKey(e => e.Id);

			// Index cho ApplicationId (unique vì 1-1)
			entity.HasIndex(e => e.ApplicationId)
				.IsUnique()
				.HasDatabaseName("IX_JobApplicationMatches_ApplicationId");

			// Index cho MatchPercentage (để lọc/sắp xếp theo tỷ lệ match)
			entity.HasIndex(e => e.MatchPercentage)
				.HasDatabaseName("IX_JobApplicationMatches_MatchPercentage");

			// Index cho CalculatedAt (lọc theo thời gian tính toán)
			entity.HasIndex(e => e.CalculatedAt)
				.HasDatabaseName("IX_JobApplicationMatches_CalculatedAt");

			// Composite index cho tìm kiếm nâng cao
			entity.HasIndex(e => new { e.ApplicationId, e.MatchPercentage })
				.HasDatabaseName("IX_JobApplicationMatches_AppId_MatchPct");

			// Cấu hình các thuộc tính
			entity.Property(e => e.Id)
				.HasDefaultValueSql("NEWID()");

			entity.Property(e => e.ApplicationId)
				.IsRequired();

			entity.Property(e => e.MatchPercentage)
				.IsRequired()
				.HasDefaultValue(0);

			// Giới hạn MatchPercentage trong khoảng 0-100 (có thể dùng CHECK constraint)
			entity.ToTable(t => t.HasCheckConstraint("CK_MatchPercentage_Range", "[MatchPercentage] BETWEEN 0 AND 100"));

			entity.Property(e => e.RequiredSkillCount)
				.IsRequired()
				.HasDefaultValue(0);

			entity.Property(e => e.MatchedSkillCount)
				.IsRequired()
				.HasDefaultValue(0);

			entity.Property(e => e.MatchedSkillsJson)
				.IsRequired(false)
				.HasMaxLength(4000) // JSON string, có thể điều chỉnh
				.HasColumnType("nvarchar(max)"); // Hoặc dùng nvarchar(max) cho JSON dài

			entity.Property(e => e.MissingSkillsJson)
				.IsRequired(false)
				.HasMaxLength(4000)
				.HasColumnType("nvarchar(max)");

			entity.Property(e => e.CalculatedAt)
				.IsRequired()
				.HasDefaultValueSql("GETUTCDATE()");

			// Quan hệ với JobApplication
			entity.HasOne(e => e.Application)
				.WithOne(a => a.Match)
				.HasForeignKey<JobApplicationMatch>(e => e.ApplicationId)
				.OnDelete(DeleteBehavior.Cascade)
				.HasConstraintName("FK_JobApplicationMatches_Application");
		}
	}
}