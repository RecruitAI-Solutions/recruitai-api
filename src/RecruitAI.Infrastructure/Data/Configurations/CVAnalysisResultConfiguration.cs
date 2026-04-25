// RecruitAI.Infrastructure/Data/Configurations/CVAnalysisResultConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Infrastructure.Data.Configurations
{
	public class CVAnalysisResultConfiguration : IEntityTypeConfiguration<CVAnalysisResult>
	{
		public void Configure(EntityTypeBuilder<CVAnalysisResult> entity)
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Confidence)
				.HasColumnType("float");

			entity.Property(e => e.CreatedAt)
				.HasDefaultValueSql("GETUTCDATE()");

			entity.HasOne(e => e.CV)
				.WithMany(c => c.AnalysisResults)
				.HasForeignKey(e => e.CVId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(e => e.Skill)
				.WithMany()
				.HasForeignKey(e => e.SkillId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasIndex(e => e.CVId)
				.HasDatabaseName("IX_CVAnalysisResults_CVId");

			entity.HasIndex(e => e.SkillId)
				.HasDatabaseName("IX_CVAnalysisResults_SkillId");

			entity.HasIndex(e => new { e.CVId, e.SkillId })
				.IsUnique()
				.HasDatabaseName("IX_CVAnalysisResults_CVId_SkillId");
		}
	}
}