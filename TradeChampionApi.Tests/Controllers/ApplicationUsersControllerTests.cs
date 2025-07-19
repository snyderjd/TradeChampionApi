using Xunit;
using Microsoft.EntityFrameworkCore;
using TradeChampionApi.Controllers;
using TradeChampionApi.Data;
using TradeChampionApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ApplicationUsersControllerTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetApplicationUsersAsync_ReturnsAllUsers()
    {
        var db = GetDbContext();
        db.ApplicationUsers.Add(new ApplicationUser { FirstName = "Test", LastName = "User", Email = "test@example.com" });
        db.ApplicationUsers.Add(new ApplicationUser { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" });
        await db.SaveChangesAsync();

        var controller = new ApplicationUsersController(db);
        var result = await controller.GetApplicationUsersAsync();

        var users = Assert.IsType<List<ApplicationUser>>(result.Value);
        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task GetApplicationUserByIdAsync_ReturnsUserWhenExists()
    {
        var db = GetDbContext();
        var user = new ApplicationUser { FirstName = "Test", LastName = "User", Email = "test@example.com" };
        db.ApplicationUsers.Add(user);
        await db.SaveChangesAsync();

        var controller = new ApplicationUsersController(db);
        var result = await controller.GetApplicationUserByIdAsync(user.Id);

        Assert.NotNull(result.Value);
        Assert.Equal("Test", result.Value.FirstName);
    }

    [Fact]
    public async Task CreateApplicationUserAsync_CreatesUser()
    {
        var db = GetDbContext();
        var controller = new ApplicationUsersController(db);
        var user = new ApplicationUser { FirstName = "New", LastName = "User", Email = "new@example.com" };

        var result = await controller.CreateApplicationUserAsync(user);

        var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        var createdUser = Assert.IsType<ApplicationUser>(createdResult.Value);

        Assert.Equal("New", createdUser.FirstName);
        Assert.True(createdUser.Id > 0);
    }

    [Fact]
    public async Task UpdateApplicationUserAsync_UpdatesUser()
    {
        var db = GetDbContext();
        var user = new ApplicationUser { FirstName = "Update", LastName = "User", Email = "update@example.com" };
        db.ApplicationUsers.Add(user);
        await db.SaveChangesAsync();

        var controller = new ApplicationUsersController(db);
        user.FirstName = "Updated";
        var result = await controller.UpdateApplicationUserAsync(user.Id, user);

        Assert.IsType<NoContentResult>(result);
        var updatedUser = await db.ApplicationUsers.FindAsync(user.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal("Updated", updatedUser.FirstName);
    }

    [Fact]
    public async Task DeleteApplicationUserAsync_RemovesUser()
    {
        var db = GetDbContext();
        var user = new ApplicationUser { FirstName = "Delete", LastName = "Me", Email = "delete@example.com" };
        db.ApplicationUsers.Add(user);
        await db.SaveChangesAsync();

        var controller = new ApplicationUsersController(db);
        var result = await controller.DeleteApplicationUserAsync(user.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await db.ApplicationUsers.FindAsync(user.Id));
    }
}