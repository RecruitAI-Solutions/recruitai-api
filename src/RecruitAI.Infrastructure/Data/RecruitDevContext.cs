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

	public virtual DbSet<Test> Tests { get; set; }
	public DbSet<User> Users { get; set; }
	public DbSet<AuthProvider> AuthProviders { get; set; }
	public DbSet<RefreshToken> RefreshTokens { get; set; }
	public DbSet<CV> CVs { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// Cấu hình cho Test
		modelBuilder.Entity<Test>(entity =>
		{
			entity.HasKey(e => e.Id).HasName("PK__Tests__3214EC071C177EA5");
			entity.Property(e => e.FirstName).HasMaxLength(50);
			entity.Property(e => e.LastName).HasMaxLength(50);
		});

		// ===== CẤU HÌNH CHO USER =====
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

			// THÊM: Cấu hình cho Role
			entity.Property(e => e.Role)
				.IsRequired()
				.HasConversion<int>() // Chuyển enum thành int để lưu trong DB
				.HasDefaultValue(UserRole.CANDIDATE); // Mặc định là Candidate

			entity.Property(e => e.PermissionCodes)
				.HasMaxLength(1000)
				.IsRequired(false);

			// SỬA LỖI: Thêm HasConversion trước khi set default value
			entity.Property(e => e.Status)
				.IsRequired()
				.HasConversion<int>() // Chuyển enum thành int
				.HasDefaultValue(UserStatus.PendingVerification); // Dùng enum, EF sẽ tự chuyển

			entity.Property(e => e.Gender)
				.HasConversion<int?>() // Cho phép null
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

		// ===== CẤU HÌNH CHO AUTH PROVIDER =====
		modelBuilder.Entity<AuthProvider>(entity =>
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
		});

		// ===== CẤU HÌNH CHO REFRESH TOKEN =====
		modelBuilder.Entity<RefreshToken>(entity =>
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
				.HasDefaultValue(TokenType.RefreshToken); // Dùng enum thay vì (int)

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
		});

		modelBuilder.Entity<PasswordResetToken>(entity =>
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
		});

		modelBuilder.Entity<CV>(entity =>
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
		});

		OnModelCreatingPartial(modelBuilder);
	}

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}