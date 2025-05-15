using Microsoft.EntityFrameworkCore;
using TradeChampionApi.Interfaces;
using TradeChampionApi.Models;

namespace TradeChampionApi.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

    public DbSet<Account> Accounts { get; set; }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Trade> Trades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Order - configure enum types 
        modelBuilder.Entity<Order>()
            .Property(o => o.OrderType)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .Property(o => o.Side)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();

        // Trade - define BuyOrder and SellOrder relationships
        modelBuilder.Entity<Trade>()
            .HasOne(t => t.BuyOrder)
            .WithMany()
            .HasForeignKey(t => t.BuyOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Trade>()
            .HasOne(t => t.SellOrder)
            .WithMany()
            .HasForeignKey(t => t.SellOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override int SaveChanges()
    {
        SetTimestamps();

        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();

        return await base.SaveChangesAsync(cancellationToken);
    }

    public void SetTimestamps()
    {
        var entries = ChangeTracker.Entries<IHasTimestamps>();

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }

}
