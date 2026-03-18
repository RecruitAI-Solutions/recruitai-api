using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Infrastructure.Data.Configurations
{
	public class CVConfiguration : IEntityTypeConfiguration<CV>
	{
		public void Configure(EntityTypeBuilder<CV> entity)
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.FileName)
				.IsRequired()
				.HasMaxLength(255);

			entity.Property(e => e.StoredFileName)
				.IsRequired()
				.HasMaxLength(255);

			entity.Property(e => e.FilePath)
				.IsRequired()
				.HasMaxLength(500);

			entity.Property(e => e.ContentType)
				.IsRequired()
				.HasMaxLength(100);

			entity.Property(e => e.Status)
				.HasConversion<int>()
				.HasDefaultValue(CVStatus.Pending);

			entity.Property(e => e.UploadedAt)
				.HasDefaultValueSql("GETUTCDATE()");

			entity.HasIndex(e => e.UserId);
			entity.HasIndex(e => e.Status);

			entity.HasOne(e => e.User)
				.WithMany(u => u.CVs)
				.HasForeignKey(e => e.UserId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}