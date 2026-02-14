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
	// Thêm các DbSet khác: public virtual DbSet<User> Users { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// Cấu hình cho từng entity
		modelBuilder.Entity<Test>(entity =>
		{
			entity.HasKey(e => e.Id).HasName("PK__Tests__3214EC071C177EA5");
			entity.Property(e => e.FirstName).HasMaxLength(50);
			entity.Property(e => e.LastName).HasMaxLength(50);
		});

		// Gọi phương thức partial để cho phép mở rộng
		OnModelCreatingPartial(modelBuilder);
	}

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}