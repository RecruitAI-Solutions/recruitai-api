using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Infrastructure.Data.Configurations
{
	public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
	{
		public void Configure(EntityTypeBuilder<RefreshToken> entity)
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Token)
				.IsRequired()
				.HasMaxLength(500);

			entity.Property(e => e.CreatedByIp)
				.HasMaxLength(50);

			entity.Property(e => e.TokenType)
				.IsRequired()
				.HasConversion<int>()
				.HasDefaultValue(TokenType.RefreshToken);

			entity.Property(e => e.CreatedAt)
				.HasDefaultValueSql("GETUTCDATE()");

			entity.Property(e => e.RevokedAt)
				.IsRequired(false);

			entity.Property(e => e.RevokedByIp)
				.HasMaxLength(50)
				.IsRequired(false);

			entity.Property(e => e.ReplacedByToken)
				.HasMaxLength(500)
				.IsRequired(false);

			entity.HasOne(e => e.User)
				.WithMany(u => u.RefreshTokens)
				.HasForeignKey(e => e.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasIndex(e => e.Token)
				.IsUnique()
				.HasDatabaseName("IX_RefreshTokens_Token");

			entity.HasIndex(e => e.ExpireAt)
				.HasDatabaseName("IX_RefreshTokens_ExpireAt");
		}
	}
}