using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Infrastructure.Configurations
{
	public class CompanyConfiguration : IEntityTypeConfiguration<Company>
	{
		public void Configure(EntityTypeBuilder<Company> builder)
		{
			builder.ToTable("Companies");

			builder.HasKey(c => c.Id);

			builder.Property(c => c.Name)
				.IsRequired()
				.HasMaxLength(200);

			builder.HasIndex(c => c.Name)
				.IsUnique();  // Tên công ty không trùng lặp

			builder.Property(c => c.Slug)
				.HasMaxLength(200);

			builder.HasIndex(c => c.Slug)
				.IsUnique();

			builder.Property(c => c.Logo)
				.HasMaxLength(500);

			builder.Property(c => c.Address)
				.HasMaxLength(500);

			builder.Property(c => c.Website)
				.HasMaxLength(500);

			builder.Property(c => c.CreatedAt)
				.IsRequired()
				.HasDefaultValueSql("GETUTCDATE()");
			builder.Property(c => c.UpdatedAt)
				.IsRequired(false);

			// Relationship: Company - Jobs
			builder.HasMany(c => c.Jobs)
				.WithOne(j => j.Company)
				.HasForeignKey(j => j.CompanyId)
				.OnDelete(DeleteBehavior.SetNull);  // Xóa công ty thì job vẫn giữ, chỉ null CompanyId

			// Relationship: Company - User (Creator)
			builder.HasOne(c => c.Creator)
				.WithMany()
				.HasForeignKey(c => c.CreatedBy)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}