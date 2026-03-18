using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Infrastructure.Data.Configurations
{
	public class AuthProviderConfiguration : IEntityTypeConfiguration<AuthProvider>
	{
		public void Configure(EntityTypeBuilder<AuthProvider> entity)
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Provider)
				.IsRequired()
				.HasConversion<int>();

			entity.Property(e => e.ProviderUserId)
				.IsRequired()
				.HasMaxLength(255);

			entity.Property(e => e.ProviderEmail)
				.HasMaxLength(256);

			entity.Property(e => e.PasswordHash)
				.HasMaxLength(255);

			entity.Property(e => e.CreatedAt)
				.HasDefaultValueSql("GETUTCDATE()");

			entity.Property(e => e.LastLoginAt)
				.IsRequired(false);

			entity.HasOne(e => e.User)
				.WithMany(u => u.AuthProviders)
				.HasForeignKey(e => e.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasIndex(e => new { e.Provider, e.ProviderUserId })
				.IsUnique()
				.HasDatabaseName("IX_AuthProviders_Provider_ProviderUserId");
		}
	}
}