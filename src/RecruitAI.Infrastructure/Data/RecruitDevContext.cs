using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Entities;
using RecruitAI.Domain.Enums;

namespace RecruitAI.Infrastructure.Data;

public partial class RecruitDevContext : DbContext
{
	public RecruitDevContext(DbContextOptions<RecruitDevContext> options)
		: base(options)
	{
	}

	// DbSets - các bảng trong database
	public virtual DbSet<Test> Tests { get; set; }
	public DbSet<User> Users { get; set; }
	public DbSet<AuthProvider> AuthProviders { get; set; }
	public DbSet<RefreshToken> RefreshTokens { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// Cấu hình cho Test
		modelBuilder.Entity<Test>(entity =>
		{
			entity.HasKey(e => e.Id).HasName("PK__Tests__3214EC071C177EA5");
			entity.Property(e => e.FirstName).HasMaxLength(50);
			entity.Property(e => e.LastName).HasMaxLength(50);
		});

		// ===== CẤU HÌNH CHO USER (ĐÃ CẬP NHẬT) =====
		modelBuilder.Entity<User>(entity =>
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

			// 👇 CÁC TRƯỜNG MỚI THÊM
			entity.Property(e => e.Status)
				.IsRequired()
				.HasDefaultValue(UserStatus.PendingVerification); // UserStatus.PendingVerification

			entity.Property(e => e.Gender)
				.IsRequired(false);

			entity.Property(e => e.DateOfBirth)
				.IsRequired(false);

			entity.Property(e => e.PhoneNumber)
				.HasMaxLength(20)
				.IsRequired(false);

			entity.Property(e => e.AvatarUrl)
				.HasMaxLength(500)
				.IsRequired(false);

			// Index cho Email
			entity.HasIndex(e => e.Email)
				.IsUnique()
				.HasDatabaseName("IX_Users_Email");
		});

		// ===== CẤU HÌNH CHO AUTH PROVIDER (ĐÃ CẬP NHẬT) =====
		modelBuilder.Entity<AuthProvider>(entity =>
		{
			entity.HasKey(e => e.Id);

			// 👇 GIỜ LÀ INT - KHỚP VỚI ENUM
			entity.Property(e => e.Provider)
				.IsRequired()
				.HasConversion<int>(); // Chuyển Enum thành int

			entity.Property(e => e.ProviderUserId)
				.HasMaxLength(255);

			entity.Property(e => e.PasswordHash)
				.HasMaxLength(255);

			entity.Property(e => e.CreatedAt)
				.HasDefaultValueSql("GETUTCDATE()");

			entity.Property(e => e.LastLoginAt)
				.IsRequired(false);

			// Relationship
			entity.HasOne(e => e.User)
				.WithMany(u => u.AuthProviders)
				.HasForeignKey(e => e.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			// Index cho Provider + ProviderUserId (đã sửa)
			entity.HasIndex(e => new { e.Provider, e.ProviderUserId })
				.IsUnique()
				.HasDatabaseName("IX_AuthProviders_Provider_ProviderUserId");
		});

		// ===== CẤU HÌNH CHO REFRESH TOKEN (ĐÃ CẬP NHẬT) =====
		modelBuilder.Entity<RefreshToken>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Token)
				.IsRequired()
				.HasMaxLength(500);

			entity.Property(e => e.CreatedByIp)
				.HasMaxLength(50);

			// 👇 THÊM TOKEN TYPE
			entity.Property(e => e.TokenType)
				.IsRequired()
				.HasConversion<int>() // Chuyển Enum thành int
				.HasDefaultValue(TokenType.RefreshToken); // RefreshToken

			entity.Property(e => e.CreatedAt)
				.HasDefaultValueSql("GETUTCDATE()");

			entity.Property(e => e.RevokedAt)
				.IsRequired(false);

			entity.Property(e => e.RevokedByIp)
				.HasMaxLength(50)
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
		});

		OnModelCreatingPartial(modelBuilder);
	}

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}