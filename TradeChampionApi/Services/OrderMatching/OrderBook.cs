using System;
using System.Collections.Generic;
using System.Linq;
using TradeChampionApi.Models;

namespace TradeChampionApi.Services.OrderMatching;

public class OrderBook
{
    private readonly string _ticker;
    private readonly SortedSet<Order> _buyOrders;
    private readonly SortedSet<Order> _sellOrders;

    public OrderBook(string ticker)
    {
        _ticker = ticker;

        // Buy orders - sort by price (high), then by createdAt
        _buyOrders = new SortedSet<Order>(new BuyOrderComparer());

        // Sell orders - sort by price (low), then by createdAt
        _sellOrders = new SortedSet<Order>(new SellOrderComparer());
    }

    public void AddOrder(Order order)
    {
        if (order.OrderType != "Limit")
            throw new NotImplementedException("Only limit orders are supported right now.");

        if (order.Side == "Buy")
            _buyOrders.Add(order);
        else if (order.Side == "Sell")
            _sellOrders.Add(order);
        else
            throw new ArgumentException("Order side must be Buy or Sell.");
    }

    public List<(Order BuyOrder, Order SellOrder, int Quantity, decimal Price)> MatchOrders()
    {
        var matches = new List<(Order, Order, int, decimal)>();

        while (_buyOrders.Any() && _sellOrders.Any())
        {
            var buy = _buyOrders.First();
            var sell = _sellOrders.First();

            if (buy.Price >= sell.Price)
            {
                int matchedQuantity = Math.Min(buy.Quantity, sell.Quantity);
                decimal tradePrice = (buy.Price + sell.Price) / 2m;

                matches.Add((buy, sell, matchedQuantity, tradePrice));

                // Update order quantities
                buy.Quantity -= matchedQuantity;
                sell.Quantity -= matchedQuantity;

                if (buy.Quantity == 0) _buyOrders.Remove(buy);
                if (sell.Quantity == 0) _sellOrders.Remove(sell);
            }
            else
            {
                break; // No further matches possible
            }
        }

        return matches;
    }

    public IReadOnlyCollection<Order> GetBuyOrders() => _buyOrders;
    public IReadOnlyCollection<Order> SellOrders() => _sellOrders;
}

// Comparers for sorting orders

public class BuyOrderComparer : ICompare<Order>
{
    public int Compare(Order x, Order y)
    {
        int priceComparison = y.Price.CompareTo(x.Price);
        return priceComparison != 0 ? priceComparison : x.CreatedAt.CompareTo(y.CreatedAt);
    }
}

public class SellOrderComparer : IComparer<Order>
{
    public int Compare(Order x, Order y)
    {
        int priceComparison = x.Price.CompareTo(y.Price);
        return priceComparison != 0 ? priceComparison : x.CreatedAt.CompareTo(y.CreatedAt);
    }
}