using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeChampionApi.Models;

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
    public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetUsersAsync()
    {
        return await _dbContext.Users.ToListAsync();
    }

    // GET: api/ApplicationUsers/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ApplicationUser>> GetUserByIdAsync(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        return user;
    }

}
