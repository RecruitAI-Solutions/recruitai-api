using Microsoft.EntityFrameworkCore;
using RecruitAI.Domain.Entities;
using RecruitAI.Infrastructure.Configurations;
using RecruitAI.Infrastructure.Data.Configurations;

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
	public DbSet<Job> Jobs { get; set; }
	public DbSet<Skill> Skills { get; set; }
	public DbSet<JobSkill> JobSkills { get; set; }
	public DbSet<JobApplication> JobApplications { get; set; }
	public DbSet<JobApplicationMatch> JobApplicationMatches { get; set; }
	public DbSet<AuditLog> AuditLogs { get; set; }
	public DbSet<Company> Companies { get; set; }
	public DbSet<SavedJob> SavedJobs { get; set; }
	public DbSet<Notification> Notifications { get; set; }
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// Áp dụng tất cả configurations từ các file riêng
		modelBuilder.ApplyConfiguration(new TestConfiguration());
		modelBuilder.ApplyConfiguration(new UserConfiguration());
		modelBuilder.ApplyConfiguration(new AuthProviderConfiguration());
		modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
		modelBuilder.ApplyConfiguration(new PasswordResetTokenConfiguration());
		modelBuilder.ApplyConfiguration(new CVConfiguration());
		modelBuilder.ApplyConfiguration(new JobConfiguration());
		modelBuilder.ApplyConfiguration(new SkillConfiguration());
		modelBuilder.ApplyConfiguration(new JobSkillConfiguration());
		modelBuilder.ApplyConfiguration(new JobApplicationConfiguration());
		modelBuilder.ApplyConfiguration(new JobApplicationMatchConfiguration());
		modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
		modelBuilder.ApplyConfiguration(new CompanyConfiguration());
		modelBuilder.ApplyConfiguration(new SavedJobConfiguration());
		modelBuilder.ApplyConfiguration(new NotificationConfiguration());

		OnModelCreatingPartial(modelBuilder);
	}

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}