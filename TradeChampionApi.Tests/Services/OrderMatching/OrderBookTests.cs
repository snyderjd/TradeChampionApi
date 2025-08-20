using Xunit;
using System;
using System.Linq;
using TradeChampionApi.Models;
using TradeChampionApi.Enums;
using TradeChampionApi.Services.OrderMatching;
using System.Collections.Generic;

public class OrderBookTests
{
    private static int _userId = 1;
    private static int _accountId = 1;

    private ApplicationUser CreateUser()
    {
        return new ApplicationUser
        {
            Id = _userId++,
            FirstName = "Test",
            LastName = "User",
            Email = $"user{_userId}@example.com"
        };
    }

    private Account CreateAccount(int? userId = null)
    {
        return new Account
        {
            Id = _accountId++,
            Name = "TestAccount",
            Balance = 1000m,
            ApplicationUserId = userId ?? _userId
        };
    }

    private Order CreateOrder(
        string ticker, 
        OrderType type, 
        OrderSide side, 
        decimal price, 
        int quantity, 
        int? accountId = null, 
        DateTime? createdAt = null
    )
    {
        return new Order
        {
            Ticker = ticker,
            OrderType = type,
            Side = side,
            Price = price,
            Quantity = quantity,
            Status = OrderStatus.Pending,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            AccountId = accountId ?? _accountId
        };
    }

    [Fact]
    public void AddOrder_AddsBuyAndSellOrders()
    {
        var user = CreateUser();
        var account = CreateAccount(user.Id);

        var book = new OrderBook("AAPL");
        var buyOrder = CreateOrder("AAPL", OrderType.Limit, OrderSide.Buy, 150, 10, account.Id);
        var sellOrder = CreateOrder("AAPL", OrderType.Limit, OrderSide.Sell, 155, 5, account.Id);

        book.AddOrder(buyOrder);
        book.AddOrder(sellOrder);

        Assert.Single(book.GetBuyOrders());
        Assert.Single(book.GetSellOrders());
    }

    [Fact]
    public void AddOrder_ThrowsForNonLimitOrder()
    {
        var user = CreateUser();
        var account = CreateAccount(user.Id);

        var book = new OrderBook("AAPL");
        var marketOrder = CreateOrder("AAPL", OrderType.Market, OrderSide.Buy, 150, 10, account.Id);

        Assert.Throws<NotImplementedException>(() => book.AddOrder(marketOrder));
    }

    [Fact]
    public void AddOrder_ThrowsForInvalidSide()
    {
        var user = CreateUser();
        var account = CreateAccount(user.Id);

        var book = new OrderBook("AAPL");
        var invalidOrder = CreateOrder("AAPL", OrderType.Limit, (OrderSide)999, 150, 10, account.Id);

        Assert.Throws<ArgumentException>(() => book.AddOrder(invalidOrder));
    }

    [Fact]
    public void MatchOrders_MatchesBuyAndSellOrders()
    {
        var user = CreateUser();
        var account = CreateAccount(user.Id);

        var book = new OrderBook("AAPL");
        var buyOrder =
            CreateOrder(
                "AAPL", OrderType.Limit, OrderSide.Buy, 155, 10, account.Id, DateTime.UtcNow.AddSeconds(-1)
            );

        var sellOrder =
            CreateOrder("AAPL", OrderType.Limit, OrderSide.Sell, 150, 5, account.Id, DateTime.UtcNow);

        book.AddOrder(buyOrder);
        book.AddOrder(sellOrder);

        var matches = book.MatchOrders();

        Assert.Single(matches);
        var match = matches[0];
        Assert.Equal(buyOrder, match.BuyOrder);
        Assert.Equal(sellOrder, match.SellOrder);
        Assert.Equal(5, match.Quantity);
        Assert.Equal(152.5m, match.Price); // Midpoint of the buy and sell orders

        // Quantities should remain unchanged
        Assert.Equal(10, buyOrder.Quantity);
        Assert.Equal(5, sellOrder.Quantity);
    }

    [Fact]
    public void MatchOrders_NoMatchWhenPricesDoNotCross()
    {
        var user = CreateUser();
        var account = CreateAccount(user.Id);

        var book = new OrderBook("AAPL");
        var buyOrder = CreateOrder("AAPL", OrderType.Limit, OrderSide.Buy, 140, 10, account.Id, DateTime.UtcNow);
        var sellOrder = CreateOrder("AAPL", OrderType.Limit, OrderSide.Sell, 150, 5, account.Id, DateTime.UtcNow);

        book.AddOrder(buyOrder);
        book.AddOrder(sellOrder);

        var matches = book.MatchOrders();

        Assert.Empty(matches);
        Assert.Single(book.GetBuyOrders());
        Assert.Single(book.GetSellOrders());
    }
}
