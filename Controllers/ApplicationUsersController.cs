using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeChampionApi.Models;
using TradeChampionApi.Data;

namespace TradeChampionApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApplicationUsersController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ApplicationUsersController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET: /api/ApplicationUsers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetApplicationUsersAsync()
    {
        return await _dbContext.ApplicationUsers.ToListAsync();
    }

    // GET: api/ApplicationUsers/5
    [HttpGet("{id}", Name = "GetApplicationUserByIdAsync")]
    public async Task<ActionResult<ApplicationUser>> GetApplicationUserByIdAsync(int id)
    {
        var user = await _dbContext.ApplicationUsers.FindAsync(id);

        if (user == null)
            return NotFound();

        return user;
    }

    // POST: api/ApplicationUsers
    [HttpPost]
    public async Task<ActionResult<ApplicationUser>> CreateApplicationUserAsync(ApplicationUser user)
    {
        _dbContext.ApplicationUsers.Add(user);
        await _dbContext.SaveChangesAsync();

        return CreatedAtRoute("GetApplicationUserByIdAsync", new { id = user.Id }, user);
    }

    // PUT: api/ApplicationUsers/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateApplicationUserAsync(int id, ApplicationUser updatedUser)
    {
        if (id != updatedUser.Id)
            return BadRequest();

        _dbContext.Entry(updatedUser).State = EntityState.Modified;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ApplicationUserExistsAsync(id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE: api/ApplicationUsers/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApplicationUserAsync(int id)
    {
        var user = await _dbContext.ApplicationUsers.FindAsync(id);
        if (user == null)
            return NotFound();

        _dbContext.ApplicationUsers.Remove(user);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> ApplicationUserExistsAsync(int id)
    {
        return await _dbContext.ApplicationUsers.AnyAsync(e => e.Id == id);
    }
}
