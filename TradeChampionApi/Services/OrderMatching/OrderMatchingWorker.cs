using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeChampionApi.Data;
using TradeChampionApi.Models;
using TradeChampionApi.Enums;

namespace TradeChampionApi.Services.OrderMatching;

public class OrderMatchingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderMatchingWorker> _logger;
    private readonly TimeSpan _matchingInterval = TimeSpan.FromMinutes(5);
    private readonly Dictionary<string, OrderBook> _orderBooks = new();

    private DateTime _lastMatchTime = DateTime.MinValue;

    public OrderMatchingWorker(IServiceProvider serviceProvider, ILogger<OrderMatchingWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderMatchingWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var matchingService = scope.ServiceProvider.GetRequiredService<OrderMatchingService>();

                await LoadOpenOrdersAsync(dbContext, stoppingToken);
                OutputOrderBookStatus();

                if (ShouldRunMatching())
                {
                    _logger.LogInformation("Running order matching...");
                    await matchingService.RunMatchingAsync(_orderBooks, stoppingToken);
                    _lastMatchTime = DateTime.UtcNow;
                    _logger.LogInformation("Matching complete.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OrderMatchingWorker loop.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private bool ShouldRunMatching() =>
        (DateTime.UtcNow - _lastMatchTime) >= _matchingInterval;

    private async Task LoadOpenOrdersAsync(AppDbContext dbContext, CancellationToken ct)
    {
        var openOrders = await dbContext.Orders
            .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.PartiallyFilled)
            .ToListAsync(ct);

        _orderBooks.Clear(); // rebuild fresh each cycle

        foreach (var order in openOrders)
        {
            if (!_orderBooks.TryGetValue(order.Ticker, out var book))
            {
                book = new OrderBook(order.Ticker);
                _orderBooks[order.Ticker] = book;
            }

            book.AddOrder(order);
        }
    }

    private void OutputOrderBookStatus()
    {
        foreach(var (ticker, book) in _orderBooks)
        {
            _logger.LogInformation($"[OrderBook] {ticker}: BUY={book.GetBuyOrders().Count}, SELL={book.GetSellOrders().Count}");
        }
    }
}