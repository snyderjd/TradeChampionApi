using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeChampionApi.Services.OrderMatching;
using TradeChampionApi.Data;
using TradeChampionApi.Models;
using TradeChampionApi.Enums;

/// <summary>
/// Unit tests for the OrderMatchingWorker background service
/// These tests use reflection and mocking to verify internal logic and interactions
/// </summary>

public class OrderMatchingWorkerTests
{
    /// <summary>
    /// Verifies that ShouldRunMatching returns true when the matching interval has passed
    /// </summary>
    [Fact]
    public void ShouldRunMatching_ReturnsTrue_WhenIntervalPassed()
    {
        var logger = new Mock<ILogger<OrderMatchingWorker>>();
        var provider = new Mock<IServiceProvider>();
        var worker = new OrderMatchingWorker(provider.Object, logger.Object);

        // Simulate last match time 10 minutes ago (matching interval is 5 minutes)
        typeof(OrderMatchingWorker)
            .GetField("_lastMatchTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(worker, DateTime.UtcNow - TimeSpan.FromMinutes(10));

        // Call the private ShouldRunMatching method using reflection
        var result = typeof(OrderMatchingWorker)
            .GetMethod("ShouldRunMatching", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(worker, null);

        // Should return true since interval has passed
        Assert.True((bool)result);
    }

//     /// <summary>
//     /// Verifies that ShouldRunMatching returns true when the matching interval has passed.
//     /// </summary>
//     [Fact]
//     public void ShouldRunMatching_ReturnsTrue_WhenIntervalPassed()
//     {
//         var logger = new Mock<ILogger<OrderMatchingWorker>>();
//         var provider = new Mock<IServiceProvider>();
//         var worker = new OrderMatchingWorker(provider.Object, logger.Object);

//         // Simulate last match time 10 minutes ago (matching interval is 5 minutes)
//         typeof(OrderMatchingWorker)
//             .GetField("_lastMatchTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//             .SetValue(worker, DateTime.UtcNow - TimeSpan.FromMinutes(10));

//         // Call the private ShouldRunMatching method using reflection
//         var result = typeof(OrderMatchingWorker)
//             .GetMethod("ShouldRunMatching", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//             .Invoke(worker, null);

//         // Should return true since interval has passed
//         Assert.True((bool)result);
//     }   
}

// using Xunit;
// using Moq;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.DependencyInjection;
// using System;
// using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;
// using TradeChampionApi.Services.OrderMatching;
// using TradeChampionApi.Data;
// using TradeChampionApi.Models;
// using TradeChampionApi.Enums;

// /// <summary>
// /// Unit tests for the OrderMatchingWorker background service.
// /// These tests use reflection and mocking to verify internal logic and interactions.
// /// </summary>
// public class OrderMatchingWorkerTests
// {
//     /// <summary>
//     /// Verifies that ShouldRunMatching returns true when the matching interval has passed.
//     /// </summary>
//     [Fact]
//     public void ShouldRunMatching_ReturnsTrue_WhenIntervalPassed()
//     {
//         var logger = new Mock<ILogger<OrderMatchingWorker>>();
//         var provider = new Mock<IServiceProvider>();
//         var worker = new OrderMatchingWorker(provider.Object, logger.Object);

//         // Simulate last match time 10 minutes ago (matching interval is 5 minutes)
//         typeof(OrderMatchingWorker)
//             .GetField("_lastMatchTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//             .SetValue(worker, DateTime.UtcNow - TimeSpan.FromMinutes(10));

//         // Call the private ShouldRunMatching method using reflection
//         var result = typeof(OrderMatchingWorker)
//             .GetMethod("ShouldRunMatching", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//             .Invoke(worker, null);

//         // Should return true since interval has passed
//         Assert.True((bool)result);
//     }

//     /// <summary>
//     /// Verifies that LoadOpenOrdersAsync builds order books correctly from open orders.
//     /// </summary>
//     [Fact]
//     public async Task LoadOpenOrdersAsync_BuildsOrderBooksCorrectly()
//     {
//         var logger = new Mock<ILogger<OrderMatchingWorker>>();
//         var provider = new Mock<IServiceProvider>();
//         var worker = new OrderMatchingWorker(provider.Object, logger.Object);

//         // Mock DbContext and set up some open orders
//         var dbContext = new Mock<AppDbContext>();
//         var orders = new List<Order>
//         {
//             new Order { Ticker = "AAPL", Status = OrderStatus.Pending, Side = OrderSide.Buy, Price = 100, Quantity = 10 },
//             new Order { Ticker = "AAPL", Status = OrderStatus.PartiallyFilled, Side = OrderSide.Sell, Price = 105, Quantity = 5 },
//             new Order { Ticker = "GOOG", Status = OrderStatus.Pending, Side = OrderSide.Buy, Price = 200, Quantity = 2 }
//         };

//         // Use a helper to mock DbSet<Order>
//         dbContext.Setup(x => x.Orders).Returns(TestDbSet.Create(orders));

//         // Call the private LoadOpenOrdersAsync method using reflection
//         await (Task)typeof(OrderMatchingWorker)
//             .GetMethod("LoadOpenOrdersAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//             .Invoke(worker, new object[] { dbContext.Object, CancellationToken.None });

//         // Access the private _orderBooks field using reflection
//         var orderBooksField = typeof(OrderMatchingWorker)
//             .GetField("_orderBooks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

//         var orderBooks = (Dictionary<string, OrderBook>)orderBooksField.GetValue(worker);

//         // There should be two order books: one for AAPL and one for GOOG
//         Assert.Equal(2, orderBooks.Count);
//         Assert.True(orderBooks.ContainsKey("AAPL"));
//         Assert.True(orderBooks.ContainsKey("GOOG"));
//         // AAPL should have two orders, GOOG should have one
//         Assert.Equal(2, orderBooks["AAPL"].GetBuyOrders().Count + orderBooks["AAPL"].GetSellOrders().Count);
//         Assert.Single(orderBooks["GOOG"].GetBuyOrders());
//     }

//     /// <summary>
//     /// Verifies that OutputOrderBookStatus logs the correct order book counts.
//     /// </summary>
//     [Fact]
//     public void OutputOrderBookStatus_LogsOrderBookCounts()
//     {
//         var logger = new Mock<ILogger<OrderMatchingWorker>>();
//         var provider = new Mock<IServiceProvider>();
//         var worker = new OrderMatchingWorker(provider.Object, logger.Object);

//         // Set up orderBooks with some dummy data
//         var orderBooksField = typeof(OrderMatchingWorker)
//             .GetField("_orderBooks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//         var orderBooks = new Dictionary<string, OrderBook>
//         {
//             { "AAPL", new OrderBook("AAPL") },
//             { "GOOG", new OrderBook("GOOG") }
//         };
//         orderBooks["AAPL"].AddOrder(new Order { Ticker = "AAPL", Side = OrderSide.Buy, Price = 100, Quantity = 1, OrderType = OrderType.Limit });
//         orderBooks["GOOG"].AddOrder(new Order { Ticker = "GOOG", Side = OrderSide.Sell, Price = 200, Quantity = 2, OrderType = OrderType.Limit });

//         // Set the private _orderBooks field
//         orderBooksField.SetValue(worker, orderBooks);

//         // Call the private OutputOrderBookStatus method using reflection
//         typeof(OrderMatchingWorker)
//             .GetMethod("OutputOrderBookStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//             .Invoke(worker, null);

//         // Verify that logger.LogInformation was called with expected message
//         logger.Verify(
//             l => l.Log(
//                 LogLevel.Information,
//                 It.IsAny<EventId>(),
//                 It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("[OrderBook]")),
//                 null,
//                 It.IsAny<Func<It.IsAnyType, Exception, string>>()),
//             Times.AtLeastOnce);
//     }
// }