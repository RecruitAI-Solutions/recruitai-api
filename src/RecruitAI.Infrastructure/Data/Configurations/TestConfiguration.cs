using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Infrastructure.Data.Configurations
{
	public class TestConfiguration : IEntityTypeConfiguration<Test>
	{
		public void Configure(EntityTypeBuilder<Test> entity)
		{
			entity.HasKey(e => e.Id).HasName("PK__Tests__3214EC071C177EA5");
			entity.Property(e => e.FirstName).HasMaxLength(50);
			entity.Property(e => e.LastName).HasMaxLength(50);
		}
	}
}