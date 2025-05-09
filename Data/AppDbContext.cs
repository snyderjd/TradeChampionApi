using Microsoft.EntityFrameworkCore;
using TradeChampionApi.Models;

namespace TradeChampionApi.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Trade> Trades { get; set; }
    public DbSet<Position> Positions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Account

        // ApplicationUser

        // Order

        // Position

        // Trade
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
        // add code to set timestamps for CreatedAt and UpdatedAt
    }

}
