using Xunit;
using Microsoft.EntityFrameworkCore;
using TradeChampionApi.Controllers;
using TradeChampionApi.Data;
using TradeChampionApi.Models;
using TradeChampionApi.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

public class OrdersControllerTests
{
    private AppDbContext GetDbContext(string dbName = "OrdersTestDb")
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AppDbContext(options);
    }

    private Account CreateAccount(AppDbContext db)
    {
        var user = new ApplicationUser { FirstName = "Trader", LastName = "User", Email = "trader@example.com" };
        db.ApplicationUsers.Add(user);
        db.SaveChanges();
        var account = new Account { Name = "Trading", Balance = 100m, ApplicationUserId = user.Id };
        db.Accounts.Add(account);
        db.SaveChanges();
        return account;
    }

    [Fact]
    public async Task GetOrdersAsync_ReturnsAllOrders()
    {
        var db = GetDbContext("GetOrdersAsync");
        var account = CreateAccount(db);
        db.Orders.Add(new Order { 
            AccountId = account.Id,
            Ticker = "AAPL",
            OrderType = OrderType.Limit,
            Side = OrderSide.Buy,
            Price = 150,
            Quantity = 10,
            Status = OrderStatus.Pending
        });

        db.Orders.Add(new Order {
            AccountId = account.Id,
            Ticker = "GOOG",
            OrderType = OrderType.Limit,
            Side = OrderSide.Sell,
            Price = 2000,
            Quantity = 5,
            Status = OrderStatus.Pending
        });

        await db.SaveChangesAsync();

        var controller = new OrdersController(db);
        var result = await controller.GetOrdersAsync();

        var orders = Assert.IsType<List<Order>>(result.Value);
        Assert.Equal(2, orders.Count);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ReturnsOrderWhenExists()
    {
        var db = GetDbContext("GetOrderByIdAsync");
        var account = CreateAccount(db);
        var order = new Order { 
            AccountId = account.Id,
            Ticker = "MSFT",
            OrderType = OrderType.Limit,
            Side = OrderSide.Buy,
            Price = 300,
            Quantity = 20,
            Status = OrderStatus.Pending
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var controller = new OrdersController(db);
        var result = await controller.GetOrderByIdAsync(order.Id);

        Assert.NotNull(result.Value);
        Assert.Equal("MSFT", result.Value.Ticker);
    }

    [Fact]
    public async Task CreateOrderAsync_CreatesOrder()
    {
        var db = GetDbContext("CreateOrderAsync");
        var account = CreateAccount(db);
        var controller = new OrdersController(db);
        var order = new Order {
            AccountId = account.Id,
            Ticker = "TSLA",
            OrderType = OrderType.Limit,
            Side = OrderSide.Buy,
            Price = 700,
            Quantity = 3,
            Status = OrderStatus.Pending
        };

        var result = await controller.CreateOrderAsync(order);

        var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        var createdOrder = Assert.IsType<Order>(createdResult.Value);

        Assert.Equal("TSLA", createdOrder.Ticker);
        Assert.True(createdOrder.Id > 0);
    }

    [Fact]
    public async Task UpdateOrderAsync_UpdatesOrder()
    {
        var db = GetDbContext("UpdateOrderAsync");
        var account = CreateAccount(db);
        var order = new Order {
            AccountId = account.Id,
            Ticker = "NFLX",
            OrderType = OrderType.Limit,
            Side = OrderSide.Sell,
            Price = 500,
            Quantity = 2,
            Status = OrderStatus.Pending
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var controller = new OrdersController(db);
        order.Price = 550;
        var result = await controller.UpdateOrderAsync(order.Id, order);

        Assert.IsType<NoContentResult>(result);
        var updatedOrder = await db.Orders.FindAsync(order.Id);
        Assert.NotNull(updatedOrder);
        Assert.Equal(550, updatedOrder.Price);
    }

    [Fact]
    public async Task DeleteOrderAsync_RemovesOrder()
    {
        var db = GetDbContext("DeleteOrderAsync");
        var account = CreateAccount(db);
        var order = new Order {
            AccountId = account.Id,
            Ticker = "AMZN",
            OrderType = OrderType.Market,
            Side = OrderSide.Buy,
            Price = 3300,
            Quantity = 1,
            Status = OrderStatus.Pending
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var controller = new OrdersController(db);
        var result = await controller.DeleteOrderAsync(order.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await db.Orders.FindAsync(order.Id));
    }
}
