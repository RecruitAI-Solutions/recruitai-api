using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;
using RecruitAI.Infrastructure.Data.SeedData;

namespace RecruitAI.Infrastructure.Data.Configurations
{
	public class UserConfiguration : IEntityTypeConfiguration<User>
	{
		public void Configure(EntityTypeBuilder<User> entity)
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Email)
				.IsRequired()
				.HasMaxLength(256);

			entity.Property(e => e.FullName)
				.IsRequired()
				.HasMaxLength(100);

			entity.Property(e => e.CreatedAt)
				.HasDefaultValueSql("GETUTCDATE()");

			entity.Property(e => e.Role)
				.IsRequired()
				.HasConversion<int>()
				.HasDefaultValue(UserRole.CANDIDATE);

			entity.Property(e => e.PermissionCodes)
				.HasMaxLength(1000)
				.IsRequired(false);

			entity.Property(e => e.Status)
				.IsRequired()
				.HasConversion<int>()
				.HasDefaultValue(UserStatus.PendingVerification);

			entity.Property(e => e.Gender)
				.HasConversion<int?>()
				.IsRequired(false);

			entity.Property(e => e.DateOfBirth)
				.IsRequired(false);

			entity.Property(e => e.PhoneNumber)
				.HasMaxLength(20)
				.IsRequired(false);

			entity.Property(e => e.AvatarUrl)
				.HasMaxLength(500)
				.IsRequired(false);

			entity.HasIndex(e => e.Email)
				.IsUnique()
				.HasDatabaseName("IX_Users_Email");
		}
	}
}