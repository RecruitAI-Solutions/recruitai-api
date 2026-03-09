using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Entities;

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
        // Cấu hình cho từng entity
        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tests__3214EC071C177EA5");
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
        });

        // Cấu hình cho User
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

            // Index cho Email để tìm kiếm nhanh
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
        });

        // Cấu hình cho AuthProvider
        modelBuilder.Entity<AuthProvider>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Provider)
                .IsRequired()
                .HasMaxLength(50); // 'email', 'facebook', 'google'

            entity.Property(e => e.ProviderUserId)
                .HasMaxLength(255);

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255);

            // Relationship
            entity.HasOne(e => e.User)
                .WithMany(u => u.AuthProviders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa User thì xóa AuthProvider

            // Index cho Provider + ProviderUserId
            entity.HasIndex(e => new { e.Provider, e.ProviderUserId })
                .IsUnique()
                .HasDatabaseName("IX_AuthProviders_Provider_ProviderUserId")
                .HasFilter("[ProviderUserId] IS NOT NULL");
        });

        // Cấu hình cho RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.CreatedByIp)
                .HasMaxLength(50);

            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa User thì xóa RefreshToken

            // Index cho Token
            entity.HasIndex(e => e.Token)
                .IsUnique()
                .HasDatabaseName("IX_RefreshTokens_Token");

            // Index cho ExpireAt để cleanup
            entity.HasIndex(e => e.ExpireAt)
                .HasDatabaseName("IX_RefreshTokens_ExpireAt");
        });
        // Gọi phương thức partial để cho phép mở rộng
        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}