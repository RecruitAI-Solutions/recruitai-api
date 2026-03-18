using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;
using RecruitAI.Infrastructure.Data.SeedData;

namespace RecruitAI.Infrastructure.Data.Configurations
{
	public class SkillConfiguration : IEntityTypeConfiguration<Skill>
	{
		public void Configure(EntityTypeBuilder<Skill> entity)
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Name)
				.IsRequired()
				.HasMaxLength(100);

			entity.Property(e => e.Category)
				.HasMaxLength(50)
				.IsRequired(false);

			entity.Property(e => e.Aliases)
				.HasMaxLength(500)
				.IsRequired(false);

			entity.Property(e => e.ContextKeywords)
				.HasMaxLength(500)
				.IsRequired(false);

			entity.Property(e => e.CreatedBy)
				.HasMaxLength(100)
				.IsRequired(false);

			entity.Property(e => e.UpdatedBy)
				.HasMaxLength(100)
				.IsRequired(false);

			entity.Property(e => e.CreatedAt)
				.HasDefaultValueSql("GETUTCDATE()");

			entity.Property(e => e.UpdatedAt)
				.IsRequired(false);

			entity.Property(e => e.IsActive)
				.HasDefaultValue(true);

			entity.HasIndex(e => e.Name)
				.IsUnique()
				.HasDatabaseName("IX_Skills_Name");

			entity.HasIndex(e => e.Category)
				.HasDatabaseName("IX_Skills_Category");

			entity.HasIndex(e => e.IsActive)
				.HasDatabaseName("IX_Skills_IsActive");

			entity.HasData(SkillSeedData.GetSkills());
		}
	}
}