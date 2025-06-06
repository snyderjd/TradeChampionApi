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
    [HttpGet("{id}")]
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

        return CreatedAtAction(nameof(GetApplicationUserByIdAsync), new { id = user.Id }, user);
    }
}
