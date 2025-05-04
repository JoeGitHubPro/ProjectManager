using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Config;
using ProjectManager.Helpers;
using ProjectManager.Models;
using ProjectManager.Models.CapitalManagement;

namespace ProjectManager.Data;

public partial class AppDbContext : IdentityDbContext<IdentityUser>
{
    public virtual DbSet<ProjectOrder> ProjectOrders { get; set; }
    public virtual DbSet<Project> Projects { get; set; }
    public virtual DbSet<ProjectCategory> ProjectCategories { get; set; }
    public virtual DbSet<Consumer> Consumers { get; set; }

    //Capital Management
    public DbSet<LoanModel> Loans { get; set; }
    public DbSet<CashFlowModel> CashFlows { get; set; }
    public DbSet<BusinessBalanceModel> BusinessBalances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdentityUser>(entity =>
        {
            entity.ToTable("Users");

        });

        modelBuilder.Entity<IdentityRole>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

        modelBuilder.Entity<Consumer>()
            .HasIndex(u => u.Name)
            .IsUnique();

        modelBuilder.Entity<ProjectOrder>()
            .HasOne(po => po.Consumer)
            .WithMany(c => c.ProjectOrders)
            .HasForeignKey(po => po.ConsumerId);

        modelBuilder.Entity<ProjectOrder>()
            .HasOne(po => po.Project)
            .WithMany(p => p.ProjectOrders)
            .HasForeignKey(po => po.ProjectId);

        OnModelCreatingPartial(modelBuilder);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = ConfigHelper.LoadConfig<AppConfig>("jsconfig.json");

        optionsBuilder.UseSqlServer(config.ConnectionString);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}