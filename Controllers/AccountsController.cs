using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeChampionApi.Models;
using TradeChampionApi.Data;

namespace TradeChampionApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AccountsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET: /api/Accounts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Account>>> GetAccountsAsync()
    {
        return await _dbContext.Accounts.ToListAsync();
    }

    // GET: /api/Accounts/5
    [HttpGet("{id}", Name = "GetAccountByIdAsync")]
    public async Task<ActionResult<Account>> GetAccountByIdAsync(int id)
    {
        var account = await _dbContext.Accounts.FindAsync(id);

        if (account == null)
            return NotFound();

        return account;
    }

    // POST: api/Accounts
    [HttpPost]
    public async Task<ActionResult<Account>> CreateAccountAsync(Account account)
    {
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        return CreatedAtRoute("GetAccountByIdAsync", new { id = account.Id }, account);
    }

    // PUT: api/Accounts/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccountAsync(int id, Account updatedAccount)
    {
        if (id != updatedAccount.Id)
            return BadRequest();

        _dbContext.Entry(updatedAccount).State = EntityState.Modified;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await AccountExistsAsync(id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE: api/Accounts/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccountAsync(int id)
    {
        var account = await _dbContext.Accounts.FindAsync(id);
        if (account == null)
            return NotFound();

        _dbContext.Accounts.Remove(account);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> AccountExistsAsync(int id)
    {
        return await _dbContext.Accounts.AnyAsync(e => e.Id == id);
    }
}