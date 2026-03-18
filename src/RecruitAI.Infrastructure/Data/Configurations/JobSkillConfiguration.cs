// RecruitAI.Infrastructure/Data/Configurations/JobSkillConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Infrastructure.Data.Configurations
{
	public class JobSkillConfiguration : IEntityTypeConfiguration<JobSkill>
	{
		public void Configure(EntityTypeBuilder<JobSkill> entity)
		{
			// Composite primary key
			entity.HasKey(e => new { e.JobId, e.SkillId });

			// Relationships
			entity.HasOne(e => e.Job)
				.WithMany(j => j.JobSkills)
				.HasForeignKey(e => e.JobId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(e => e.Skill)
				.WithMany(s => s.JobSkills)
				.HasForeignKey(e => e.SkillId)
				.OnDelete(DeleteBehavior.Restrict); // Không cho xóa skill đang được dùng

			// Indexes
			entity.HasIndex(e => e.SkillId)
				.HasDatabaseName("IX_JobSkills_SkillId");
		}
	}
}