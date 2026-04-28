using Microsoft.EntityFrameworkCore;

namespace VinhKhanhAdmin.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<PointOfInterest> Pois => Set<PointOfInterest>();
    public DbSet<ActiveDevice> ActiveDevices => Set<ActiveDevice>();
    public DbSet<NarrationLog> NarrationLogs => Set<NarrationLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        b.Entity<ActiveDevice>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        b.Entity<NarrationLog>()
            .HasOne(n => n.Poi)
            .WithMany()
            .HasForeignKey(n => n.PoiId)
            .OnDelete(DeleteBehavior.NoAction);

        // Tell EF Core 8 that PointsOfInterest has a database trigger
        // (required for SaveChanges to work correctly with SQL Server triggers)
        b.Entity<PointOfInterest>()
            .ToTable(tb => tb.HasTrigger("tr_POI_UpdateTimestamp"));

        base.OnModelCreating(b);
    }
}
