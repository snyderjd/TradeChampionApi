using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeChampionApi.Models;
using TradeChampionApi.Data;

namespace TradeChampionApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public OrdersController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET: /api/Orders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrdersAsync()
    {
        return await _dbContext.Orders.ToListAsync();
    }

    // GET: /api/Orders/5
    [HttpGet("{id}", Name = "GetOrderByIdAsync")]
    public async Task<ActionResult<Order>> GetOrderByIdAsync(int id)
    {
        var order = await _dbContext.Orders.FindAsync(id);

        if (order == null)
            return NotFound();

        return order;
    }

    // POST: api/Orders
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrderAsync(Order order)
    {
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        return CreatedAtRoute("GetOrderByIdAsync", new { id = order.Id }, order);
    }

    // PUT: api/Orders/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrderAsync(int id, Order updatedOrder)
    {
        if (id != updatedOrder.Id)
            return BadRequest();

        _dbContext.Entry(updatedOrder).State = EntityState.Modified;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await OrderExistsAsync(id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE: api/Orders/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrderAsync(int id)
    {
        var order = await _dbContext.Orders.FindAsync(id);

        if (order == null)
            return NotFound();

        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> OrderExistsAsync(int id)
    {
        return await _dbContext.Orders.AnyAsync(e => e.Id == id);
    }
}