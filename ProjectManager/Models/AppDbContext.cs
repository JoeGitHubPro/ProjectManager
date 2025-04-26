using Microsoft.EntityFrameworkCore;
using ProjectManager.Config;
using ProjectManager.Helpers;

namespace ProjectManager.Models;

public partial class AppDbContext : DbContext
{
    public virtual DbSet<ProjectOrder> ProjectOrders { get; set; }
    public virtual DbSet<Project> Projects { get; set; }
    public virtual DbSet<ProjectCategory> ProjectCategories { get; set; }
    public virtual DbSet<Consumer> Consumers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Consumer>()
            .HasIndex(u => new { u.Phone, u.CommercialRegisterNo })
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