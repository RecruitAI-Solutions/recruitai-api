using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;

namespace RecruitAI.Infrastructure.Data.Configurations
{
	public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
	{
		public void Configure(EntityTypeBuilder<PasswordResetToken> entity)
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Token)
				.IsRequired()
				.HasMaxLength(500);

			entity.Property(e => e.CreatedByIp)
				.HasMaxLength(50);

			entity.HasOne(e => e.User)
				.WithMany()
				.HasForeignKey(e => e.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasIndex(e => e.Token)
				.IsUnique()
				.HasDatabaseName("IX_PasswordResetTokens_Token");

			entity.HasIndex(e => e.ExpiryDate)
				.HasDatabaseName("IX_PasswordResetTokens_ExpiryDate");
		}
	}
}