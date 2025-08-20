using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeChampionApi.Data;
using TradeChampionApi.Models;
using TradeChampionApi.Enums;
using TradeChampionApi.Services.OrderMatching;

namespace TradeChampionApi.Services.OrderMatching;

public class OrderMatchingService
{
    private readonly AppDbContext _dbContext;

    public OrderMatchingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RunMatchingAsync(Dictionary<string, OrderBook> orderBooks, CancellationToken ct = default)
    {
        var supportsTransactions = _dbContext.Database.IsRelational();

        if (supportsTransactions)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

            try
            {
                await MatchAndCreateTrades(orderBooks, ct);
                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
        else
        {
            await MatchAndCreateTrades(orderBooks, ct);
            await _dbContext.SaveChangesAsync(ct);
        }


    }

    private async Task MatchAndCreateTrades(Dictionary<string, OrderBook> orderBooks, CancellationToken ct)
    {
        foreach (var book in orderBooks.Values)
        {
            var matches = book.MatchOrders();
            foreach (var (buy, sell, quantity, price) in matches)
            {
                await CreateTradeAsync(buy, sell, quantity, price, ct);
            }
        }
    }

    public async Task CreateTradeAsync(Order buyOrder, Order sellOrder, int quantity, decimal price, CancellationToken ct)
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

        // Update the orders' quantities and statuses
        buyOrder.Quantity -= quantity;
        sellOrder.Quantity -= quantity;

        buyOrder.Status = buyOrder.Quantity == 0 ? OrderStatus.Filled : OrderStatus.PartiallyFilled;
        sellOrder.Status = sellOrder.Quantity == 0 ? OrderStatus.Filled : OrderStatus.PartiallyFilled;

        await UpdatePositionAsync(buyOrder.AccountId, buyOrder.Ticker, quantity, price, ct);
        await UpdatePositionAsync(sellOrder.AccountId, sellOrder.Ticker, -quantity, price, ct);
    }

    public async Task UpdatePositionAsync(
        int accountId, string ticker, int quantityDelta, decimal price, CancellationToken ct
    )
    {
        var position =
            await _dbContext.Positions.FirstOrDefaultAsync(p => p.AccountId == accountId && p.Ticker == ticker, ct);

        if (position != null)
        {
            // If the new quantity is zero, delete the position
            if (position.Quantity + quantityDelta == 0)
            {
                _dbContext.Positions.Remove(position);
                return;
            }

            // Update the position's average price and quantity
            position.AveragePrice =
                (position.AveragePrice * position.Quantity + price * quantityDelta) / (position.Quantity + quantityDelta);

            position.Quantity += quantityDelta;
        }
        else
        {
            _dbContext.Positions.Add(new Position
            {
                AccountId = accountId,
                Ticker = ticker,
                Quantity = quantityDelta,
                AveragePrice = price
            });
        }
    }
}
