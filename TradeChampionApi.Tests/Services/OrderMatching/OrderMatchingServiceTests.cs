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


}

// using Xunit;
// using Microsoft.EntityFrameworkCore;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using TradeChampionApi.Data;
// using TradeChampionApi.Models;
// using TradeChampionApi.Enums;
// using TradeChampionApi.Services.OrderMatching;
// using System.Threading;

// public class OrderMatchingServiceTests
// {
//     private AppDbContext GetDbContext()
//     {
//         var options = new DbContextOptionsBuilder<AppDbContext>()
//             .UseInMemoryDatabase(databaseName: "OrderMatchingTestDb")
//             .Options;
//         return new AppDbContext(options);
//     }

//     private ApplicationUser CreateUser()
//     {
//         return new ApplicationUser
//         {
//             FirstName = "Test",
//             LastName = "User",
//             Email = "user@example.com"
//         };
//     }

//     private Account CreateAccount(ApplicationUser user)
//     {
//         return new Account
//         {
//             Name = "TestAccount",
//             Balance = 1000m,
//             ApplicationUser = user,
//             ApplicationUserId = user.Id
//         };
//     }

//     private Order CreateOrder(int accountId, string ticker, OrderType type, OrderSide side, decimal price, int quantity)
//     {
//         return new Order
//         {
//             AccountId = accountId,
//             Ticker = ticker,
//             OrderType = type,
//             Side = side,
//             Price = price,
//             Quantity = quantity,
//             Status = OrderStatus.Open,
//             CreatedAt = System.DateTime.UtcNow
//         };
//     }

//     [Fact]
//     public async Task RunMatchingAsync_CreatesTradeAndUpdatesOrdersAndPositions()
//     {
//         var db = GetDbContext();
//         var user = CreateUser();
//         db.ApplicationUsers.Add(user);
//         db.SaveChanges();
//         var account = CreateAccount(user);
//         db.Accounts.Add(account);
//         db.SaveChanges();

//         var buyOrder = CreateOrder(account.Id, "AAPL", OrderType.Limit, OrderSide.Buy, 155, 10);
//         var sellOrder = CreateOrder(account.Id, "AAPL", OrderType.Limit, OrderSide.Sell, 150, 10);
//         db.Orders.Add(buyOrder);
//         db.Orders.Add(sellOrder);
//         db.SaveChanges();

//         var orderBook = new OrderBook("AAPL");
//         orderBook.AddOrder(buyOrder);
//         orderBook.AddOrder(sellOrder);

//         var orderBooks = new Dictionary<string, OrderBook> { { "AAPL", orderBook } };
//         var service = new OrderMatchingService(db);

//         await service.RunMatchingAsync(orderBooks, CancellationToken.None);

//         // Assert trade created
//         var trade = await db.Trades.FirstOrDefaultAsync();
//         Assert.NotNull(trade);
//         Assert.Equal("AAPL", trade.Ticker);
//         Assert.Equal(10, trade.Quantity);

//         // Assert orders updated
//         var updatedBuyOrder = await db.Orders.FindAsync(buyOrder.Id);
//         var updatedSellOrder = await db.Orders.FindAsync(sellOrder.Id);
//         Assert.Equal(OrderStatus.Filled, updatedBuyOrder.Status);
//         Assert.Equal(OrderStatus.Filled, updatedSellOrder.Status);
//         Assert.Equal(0, updatedBuyOrder.Quantity);
//         Assert.Equal(0, updatedSellOrder.Quantity);

//         // Assert position updated
//         var position = await db.Positions.FirstOrDefaultAsync(p => p.AccountId == account.Id && p.Ticker == "AAPL");
//         Assert.NotNull(position);
//         Assert.Equal(0, position.Quantity); // Position removed if quantity is zero
//     }

//     [Fact]
//     public async Task UpdatePositionAsync_AddsNewPosition_WhenNoneExists()
//     {
//         var db = GetDbContext();
//         var service = new OrderMatchingService(db);

//         await service.UpdatePositionAsync(1, "GOOG", 5, 100, CancellationToken.None);

//         var position = await db.Positions.FirstOrDefaultAsync(p => p.AccountId == 1 && p.Ticker == "GOOG");
//         Assert.NotNull(position);
//         Assert.Equal(5, position.Quantity);
//         Assert.Equal(100, position.AveragePrice);
//     }

//     [Fact]
//     public async Task UpdatePositionAsync_UpdatesExistingPosition()
//     {
//         var db = GetDbContext();
//         db.Positions.Add(new Position { AccountId = 1, Ticker = "MSFT", Quantity = 10, AveragePrice = 200 });
//         db.SaveChanges();

//         var service = new OrderMatchingService(db);
//         await service.UpdatePositionAsync(1, "MSFT", 5, 220, CancellationToken.None);

//         var position = await db.Positions.FirstOrDefaultAsync(p => p.AccountId == 1 && p.Ticker == "MSFT");
//         Assert.NotNull(position);
//         Assert.Equal(15, position.Quantity);
//         Assert.True(position.AveragePrice > 200 && position.AveragePrice < 220);
//     }

//     [Fact]
//     public async Task UpdatePositionAsync_RemovesPosition_WhenQuantityZero()
//     {
//         var db = GetDbContext();
//         db.Positions.Add(new Position { AccountId = 1, Ticker = "TSLA", Quantity = 5, AveragePrice = 300 });
//         db.SaveChanges();

//         var service = new OrderMatchingService(db);
//         await service.UpdatePositionAsync(1, "TSLA", -5, 300, CancellationToken.None);

//         var position = await db.Positions.FirstOrDefaultAsync(p => p.AccountId == 1 && p.Ticker == "TSLA");
//         Assert.Null(position);
//     }
// }