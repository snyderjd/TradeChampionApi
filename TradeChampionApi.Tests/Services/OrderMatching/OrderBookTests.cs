using Xunit;
using System;
using System.Linq;
using TradeChampionApi.Models;
using TradeChampionApi.Enums;
using TradeChampionApi.Services.OrderMatching;
using System.Collections.Generic;

public class OrderBookTests
{
    
}





// using Xunit;
// using System;
// using System.Linq;
// using TradeChampionApi.Models;
// using TradeChampionApi.Enums;
// using TradeChampionApi.Services.OrderMatching;
// using System.Collections.Generic;

// public class OrderBookTests
// {
//     private Order CreateOrder(string ticker, OrderType type, OrderSide side, decimal price, int quantity, DateTime? createdAt = null)
//     {
//         return new Order
//         {
//             Ticker = ticker,
//             OrderType = type,
//             Side = side,
//             Price = price,
//             Quantity = quantity,
//             Status = OrderStatus.Pending,
//             CreatedAt = createdAt ?? DateTime.UtcNow
//         };
//     }

//     [Fact]
//     public void AddOrder_AddsBuyAndSellOrders()
//     {
//         var book = new OrderBook("AAPL");
//         var buyOrder = CreateOrder("AAPL", OrderType.Limit, OrderSide.Buy, 150, 10);
//         var sellOrder = CreateOrder("AAPL", OrderType.Limit, OrderSide.Sell, 155, 5);

//         book.AddOrder(buyOrder);
//         book.AddOrder(sellOrder);

//         Assert.Single(book.GetBuyOrders());
//         Assert.Single(book.SellOrders());
//     }

//     [Fact]
//     public void AddOrder_ThrowsForNonLimitOrder()
//     {
//         var book = new OrderBook("AAPL");
//         var marketOrder = CreateOrder("AAPL", OrderType.Market, OrderSide.Buy, 150, 10);

//         Assert.Throws<NotImplementedException>(() => book.AddOrder(marketOrder));
//     }

//     [Fact]
//     public void AddOrder_ThrowsForInvalidSide()
//     {
//         var book = new OrderBook("AAPL");
//         var invalidOrder = CreateOrder("AAPL", OrderType.Limit, (OrderSide)999, 150, 10);

//         Assert.Throws<ArgumentException>(() => book.AddOrder(invalidOrder));
//     }

//     [Fact]
//     public void MatchOrders_MatchesBuyAndSellOrders()
//     {
//         var book = new OrderBook("AAPL");
//         var buyOrder = CreateOrder("AAPL", OrderType.Limit, OrderSide.Buy, 155, 10, DateTime.UtcNow.AddSeconds(-1));
//         var sellOrder = CreateOrder("AAPL", OrderType.Limit, OrderSide.Sell, 150, 5, DateTime.UtcNow);

//         book.AddOrder(buyOrder);
//         book.AddOrder(sellOrder);

//         var matches = book.MatchOrders();

//         Assert.Single(matches);
//         var match = matches[0];
//         Assert.Equal(buyOrder, match.BuyOrder);
//         Assert.Equal(sellOrder, match.SellOrder);
//         Assert.Equal(5, match.Quantity);
//         Assert.Equal(152.5m, match.Price);
//         Assert.Equal(5, buyOrder.Quantity); // Remaining
//         Assert.Equal(0, sellOrder.Quantity); // Fully matched
//     }

//     [Fact]
//     public void MatchOrders_NoMatchWhenPricesDoNotCross()
//     {
//         var book = new OrderBook("AAPL");
//         var buyOrder = CreateOrder("AAPL", OrderType.Limit, OrderSide.Buy, 140, 10);
//         var sellOrder = CreateOrder("AAPL", OrderType.Limit, OrderSide.Sell, 150, 5);

//         book.AddOrder(buyOrder);
//         book.AddOrder(sellOrder);

//         var matches = book.MatchOrders();

//         Assert.Empty(matches);
//         Assert.Single(book.GetBuyOrders());
//         Assert.Single(book.SellOrders());
//     }
// }