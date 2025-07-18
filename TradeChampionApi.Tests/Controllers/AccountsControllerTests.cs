using Xunit;
using Microsoft.EntityFrameworkCore;
using TradeChampionApi.Controllers;
using TradeChampionApi.Data;
using TradeChampionApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

public class AccountsControllerTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "AccountsTestDb")
            .Options;
        return new AppDbContext(options);
    }

    private ApplicationUser CreateUser(AppDbContext db)
    {
        var user = new ApplicationUser { FirstName = "Owner", LastName = "User", Email = "owner@example.com" };
        db.ApplicationUsers.Add(user);
        db.SaveChanges();
        return user;
    }

    [Fact]
    public async Task GetAccountsAsync_ReturnsAllAccounts()
    {
        var db = GetDbContext();
        var user = CreateUser(db);
        db.Accounts.Add(new Account { Name = "Checking", Balance = 100m, ApplicationUserId = user.Id });
        db.Accounts.Add(new Account { Name = "Savings", Balance = 200m, ApplicationUserId = user.Id });
        await db.SaveChangesAsync();

        var controller = new AccountsController(db);
        var result = await controller.GetAccountsAsync();

        var accounts = Assert.IsType<List<Account>>(result.Value);
        Assert.Equal(2, accounts.Count);
    }

    [Fact]
    public async Task GetAccountById_ReturnsAccountWhenExists()
    {
        var db = GetDbContext();
        var user = CreateUser(db);
        var account = new Account { Name = "Investment", Balance = 500m, ApplicationUserId = user.Id };
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var controller = new AccountsController(db);
        var result = await controller.GetAccountByIdAsync(account.Id);

        Assert.NotNull(result.Value);
        Assert.Equal("Investment", result.Value.Name);
    }

    [Fact]
    public async Task CreateAccountAsync_CreatesAccount()
    {
        var db = GetDbContext();
        var user = CreateUser(db);
        var controller = new AccountsController(db);
        var account = new Account { Name = "NewAccount", Balance = 100m, ApplicationUserId = user.Id };

        var result = await controller.CreateAccountAsync(account);

        var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        var createdAccount = Assert.IsType<Account>(createdResult.Value);

        Assert.Equal("NewAccount", createdAccount.Name);
        Assert.True(createdAccount.Id > 0);
    }

    [Fact]
    public async Task UpdateAccountAsync_UpdatesAccount()
    {
        var db = GetDbContext();
        var user = CreateUser(db);
        var account = new Account { Name = "OldName", Balance = 50m, ApplicationUserId = user.Id };
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var controller = new AccountsController(db);
        account.Name = "UpdatedName";
        var result = await controller.UpdateAccountAsync(account.Id, account);

        Assert.IsType<NoContentResult>(result);
        var updatedAccount = await db.Accounts.FindAsync(account.Id);
        Assert.NotNull(updatedAccount);
        Assert.Equal("UpdatedName", updatedAccount.Name);
    }

    [Fact]
    public async Task DeleteAccountAsync_RemovesAccount()
    {
        var db = GetDbContext();
        var user = CreateUser(db);
        var account = new Account { Name = "DeleteMe", Balance = 0m, ApplicationUserId = user.Id };
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var controller = new AccountsController(db);
        var result = await controller.DeleteAccountAsync(account.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await db.Accounts.FindAsync(account.Id));        
    }
}
