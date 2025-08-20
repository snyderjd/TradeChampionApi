using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeChampionApi.Data;
using TradeChampionApi.Models;
using TradeChampionApi.Enums;
using TradeChampionApi.Services.OrderMatching;
using System.Threading;

public class OrderMatchingServiceTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "OrderMatchingTestDb")
            .Options;

        return new AppDbContext(options);
    }

    private ApplicationUser CreateUser()
    {
        return new ApplicationUser
        {
            FirstName = "Test",
            LastName = "User",
            Email = "user@example.com"
        };
    }

    private Account CreateAccount(ApplicationUser user)
    {
        return new Account
        {
            Name = "TestAccount",
            Balance = 10000m,
            ApplicationUser = user,
            ApplicationUserId = user.Id
        };
    }

    private Order CreateOrder(
        int accountId,
        string ticker,
        OrderType type,
        OrderSide side,
        decimal price,
        int quantity
    )
    {
        return new Order
        {
            AccountId = accountId,
            Ticker = ticker,
            OrderType = type,
            Side = side,
            Price = price,
            Quantity = quantity,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    private Position CreatePosition(int accountId, string ticker, int quantity, decimal averagePrice)
    {
        return new Position
        {
            AccountId = accountId,
            Ticker = ticker,
            Quantity = quantity,
            AveragePrice = averagePrice
        };
    }

    [Fact]
    public async Task RunMatchingAsync_CreatesTradeAndUpdatesOrdersAndPositions()
    {
        var db = GetDbContext();

        // Create two users and accounts
        var buyUser = CreateUser();
        var sellUser = CreateUser();
        db.ApplicationUsers.Add(buyUser);
        db.ApplicationUsers.Add(sellUser);
        db.SaveChanges();

        var buyAccount = CreateAccount(buyUser);
        var sellAccount = CreateAccount(sellUser);
        db.Accounts.Add(buyAccount);
        db.Accounts.Add(sellAccount);
        db.SaveChanges();

        // Seller already owns 10 shares of AAPL
        var sellPosition = CreatePosition(sellAccount.Id, "AAPL", 10, 150);
        db.Positions.Add(sellPosition);
        db.SaveChanges();

        // Buy order from buyAccount, sell order from sellAccount
        var buyOrder = CreateOrder(buyAccount.Id, "AAPL", OrderType.Limit, OrderSide.Buy, 155, 10);
        var sellOrder = CreateOrder(sellAccount.Id, "AAPL", OrderType.Limit, OrderSide.Sell, 150, 10);
        db.Orders.Add(buyOrder);
        db.Orders.Add(sellOrder);
        db.SaveChanges();

        var orderBook = new OrderBook("AAPL");
        orderBook.AddOrder(buyOrder);
        orderBook.AddOrder(sellOrder);

        var orderBooks = new Dictionary<string, OrderBook> { { "AAPL", orderBook } };
        var service = new OrderMatchingService(db);

        await service.RunMatchingAsync(orderBooks, CancellationToken.None);

        // Assert trade created
        var trade = await db.Trades.FirstOrDefaultAsync();
        Assert.NotNull(trade);
        Assert.Equal("AAPL", trade.Ticker);
        Assert.Equal(10, trade.Quantity);

        // Assert orders updated
        var updatedBuyOrder = await db.Orders.FindAsync(buyOrder.Id);
        var updatedSellOrder = await db.Orders.FindAsync(sellOrder.Id);
        Assert.NotNull(updatedBuyOrder);
        Assert.NotNull(updatedSellOrder);
        Assert.Equal(OrderStatus.Filled, updatedBuyOrder.Status);
        Assert.Equal(OrderStatus.Filled, updatedSellOrder.Status);
        Assert.Equal(0, updatedBuyOrder.Quantity);
        Assert.Equal(0, updatedSellOrder.Quantity);

        // Assert buyAccount position updated (should now own 10 shares)
        var buyPosition = await db.Positions.FirstOrDefaultAsync(p => p.AccountId == buyAccount.Id && p.Ticker == "AAPL");
        Assert.NotNull(buyPosition);
        Assert.Equal(10, buyPosition.Quantity);

        // Assert sellAccount position updated (should be removed since quantity is zero)
        var updatedSellPosition = await db.Positions.FirstOrDefaultAsync(p => p.AccountId == sellAccount.Id && p.Ticker == "AAPL");
        Assert.Null(updatedSellPosition);
    }

    [Fact]
    public async Task UpdatePositionAsync_AddsNewPosition_WhenNoneExists()
    {
        var db = GetDbContext();
        var service = new OrderMatchingService(db);

        await service.UpdatePositionAsync(1, "GOOG", 5, 100, CancellationToken.None);
        await db.SaveChangesAsync();

        var position = await db.Positions.FirstOrDefaultAsync(p => p.AccountId == 1 && p.Ticker == "GOOG");
        Assert.NotNull(position);
        Assert.Equal(5, position.Quantity);
        Assert.Equal(100, position.AveragePrice);
    }

    [Fact]
    public async Task UpdatePositionAsync_UpdatesExistingPosition()
    {
        var db = GetDbContext(); 
        db.Positions.Add(CreatePosition(1, "MSFT", 10, 200));
        db.SaveChanges();

        var service = new OrderMatchingService(db);
        await service.UpdatePositionAsync(1, "MSFT", 5, 220, CancellationToken.None);
        await db.SaveChangesAsync();

        var position = await db.Positions.FirstOrDefaultAsync(p => p.AccountId == 1 && p.Ticker == "MSFT");
        Assert.NotNull(position);
        Assert.Equal(15, position.Quantity);
        Assert.True(position.AveragePrice > 200 && position.AveragePrice < 220);
    }

    [Fact]
    public async Task UpdatePositionAsync_RemovesPosition_WhenQuantityZero()
    {
        var db = GetDbContext();
        db.Positions.Add(CreatePosition(1, "TSLA", 5, 300));
        db.SaveChanges();

        var service = new OrderMatchingService(db);
        await service.UpdatePositionAsync(1, "TSLA", -5, 300, CancellationToken.None);
        await db.SaveChangesAsync();

        var position = await db.Positions.FirstOrDefaultAsync(p => p.AccountId == 1 && p.Ticker == "TSLA");
        Assert.Null(position);
    }
}
