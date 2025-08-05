using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeChampionApi.Models;
using TradeChampionApi.Data;
using TradeChampionApi.Enums;

namespace TradeChampionApi.Services.OrderMatchingService;

public class OrderMatchingService
{
    private readonly AppDbContext _dbContext;

    public OrderMatchingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RunMatchingAsync()
    {
        // 1. Fetch all orders that are either Pending or Partially Filled
        var openOrders = await _dbContext.Orders
            .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.PartiallyFilled)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();

        if (!openOrders.Any()) return;

        // 2. Group by symbol and create OrderBooks
        var orderBooks = new Dictionary<string, OrderBook>();

        foreach (var order in openOrders)
        {
            if (!orderBooks.TryGetValue(order.Ticker, out var book))
            {
                book = new OrderBook(order.Ticker);
                orderBooks[order.Ticker] = book;
            }

            book.AddOrder(order);
        }

        // 3. Match orders and create trades
        foreach (var kvp in orderBooks)
        {
            var ticker = kvp.Key;
            var book = kvp.Value;

            var matches = book.MatchOrders();

            foreach (var (buy, sell, quantity, price) in matches)
            {
                await CreateTradeAsync(buy, sell, quantity, price);
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task CreateTradeAsync(Order buyOrder, Order sellOrder, int quantity, decimal price)
    {
        var trade = new Trade
        {
            BuyOrderId = buyOrder.Id,
            SellOrderId = sellOrder.Id,
            Ticker = buyOrder.Ticker,
            Quantity = quantity,
            Price = price,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Trades.Add(trade);

        // Update order quantities and statuses
        buyOrder.Quantity -= quantity;
        sellOrder.Quantity -= quantity;

        if (buyOrder.Quantity == 0)
            buyOrder.Status = OrderStatus.Filled;
        else
            buyOrder.Status = OrderStatus.PartiallyFilled;

        if (sellOrder.Quantity == 0)
            sellOrder.Status = OrderStatus.Filled;
        else
            sellOrder.Status = OrderStatus.PartiallyFilled;

        // Update or create positions
        await UpdatePositionAsync(buyOrder.AccountId, buyOrder.Ticker, quantity);
        await UpdatePositionAsync(sellOrder.AccountId, sellOrder.Ticker, -quantity);
    }

    private async Task UpdatePositionAsync(int accountId, string ticker, int quantityDelta)
    {
        var position = await _dbContext.Positions.FirstOrDefaultAsync(p => p.AccountId == accountId && p.Ticker = ticker);

        if (position != null)
        {
            position.Quantity += quantityDelta;
        }
        else
        {
            position = new Position
            {
                AccountId = accountId,
                Ticker = ticker,
                Quantity = quantityDelta
            };
            
            _dbContext.Positions.Add(position);
        }
    }

}
