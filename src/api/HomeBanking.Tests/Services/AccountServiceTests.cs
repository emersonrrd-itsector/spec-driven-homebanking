namespace HomeBanking.Tests.Services;

using HomeBanking.API.Data;
using HomeBanking.API.Services;
using HomeBanking.Domain;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Unit tests for AccountService.
/// Tests GetUserAccountsAsync and GetUserAccountAsync methods.
/// </summary>
public class AccountServiceTests
{
    private readonly HomeBankingContext context;
    private readonly AccountService service;

    public AccountServiceTests()
    {
        var options = new DbContextOptionsBuilder<HomeBankingContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        this.context = new HomeBankingContext(options);
        this.service = new AccountService(this.context);
        this.SeedTestData();
    }

    [Fact]
    public async Task GetUserAccountsAsync_WithValidUserId_ReturnsAllUserAccounts()
    {
        var userId = Guid.NewGuid();
        var account1 = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Checking",
            Balance = 5000.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
        };
        var account2 = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Savings",
            Balance = 15000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
        };

        this.context.Accounts.AddRange(account1, account2);
        await this.context.SaveChangesAsync();

        var result = await this.service.GetUserAccountsAsync(userId);

        Assert.NotNull(result);
        var accounts = result.ToList();
        Assert.Equal(2, accounts.Count);
    }

    [Fact]
    public async Task GetUserAccountsAsync_WithValidUserId_ReturnsAccountsWithCorrectData()
    {
        var userId = Guid.NewGuid();
        var account1Id = Guid.NewGuid();
        var lastUpdated = DateTime.UtcNow;
        var account = new Account
        {
            Id = account1Id,
            UserId = userId,
            Name = "Checking",
            Balance = 5000.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = lastUpdated,
        };

        this.context.Accounts.Add(account);
        await this.context.SaveChangesAsync();

        var result = await this.service.GetUserAccountsAsync(userId);

        var accounts = result.ToList();
        var returnedAccount = accounts[0];
        Assert.Equal(account1Id, returnedAccount.Id);
        Assert.Equal("Checking", returnedAccount.Name);
        Assert.Equal(5000.00m, returnedAccount.Balance);
        Assert.Equal("USD", returnedAccount.Currency);
    }

    [Fact]
    public async Task GetUserAccountsAsync_WithUserIdHavingNoAccounts_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();

        var result = await this.service.GetUserAccountsAsync(userId);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserAccountsAsync_WithMultipleUsers_ReturnsOnlyUserAccounts()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var account1 = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId1,
            Name = "Checking",
            Balance = 5000.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
        };
        var account2 = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId2,
            Name = "Savings",
            Balance = 15000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
        };

        this.context.Accounts.AddRange(account1, account2);
        await this.context.SaveChangesAsync();

        var result = await this.service.GetUserAccountsAsync(userId1);

        Assert.Single(result);
        Assert.Equal("Checking", result.First().Name);
    }

    [Fact]
    public async Task GetUserAccountsAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.GetUserAccountsAsync(Guid.Empty));
    }

    [Fact]
    public async Task GetUserAccountsAsync_WithValidUserId_AccountsAreOrderedByName()
    {
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var account1 = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Zebra",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = now,
        };
        var account2 = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Alpha",
            Balance = 2000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = now,
        };

        this.context.Accounts.AddRange(account1, account2);
        await this.context.SaveChangesAsync();

        var result = await this.service.GetUserAccountsAsync(userId);

        var accounts = result.ToList();
        Assert.Equal("Alpha", accounts[0].Name);
        Assert.Equal("Zebra", accounts[1].Name);
    }

    [Fact]
    public async Task GetUserAccountAsync_WithValidUserIdAndAccountId_ReturnsAccount()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var account = new Account
        {
            Id = accountId,
            UserId = userId,
            Name = "Checking",
            Balance = 5000.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
        };

        this.context.Accounts.Add(account);
        await this.context.SaveChangesAsync();

        var result = await this.service.GetUserAccountAsync(userId, accountId);

        Assert.NotNull(result);
        Assert.Equal(accountId, result.Id);
        Assert.Equal("Checking", result.Name);
    }

    [Fact]
    public async Task GetUserAccountAsync_WithValidUserIdAndAccountId_ReturnsAccountWithCorrectData()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var lastUpdated = DateTime.UtcNow;
        var account = new Account
        {
            Id = accountId,
            UserId = userId,
            Name = "Savings",
            Balance = 15000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = lastUpdated,
        };

        this.context.Accounts.Add(account);
        await this.context.SaveChangesAsync();

        var result = await this.service.GetUserAccountAsync(userId, accountId);

        Assert.NotNull(result);
        Assert.Equal("Savings", result.Name);
        Assert.Equal(15000.00m, result.Balance);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public async Task GetUserAccountAsync_WithNonexistentAccountId_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var result = await this.service.GetUserAccountAsync(userId, accountId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserAccountAsync_WithAccountFromDifferentUser_ReturnsNull()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var account = new Account
        {
            Id = accountId,
            UserId = userId1,
            Name = "Checking",
            Balance = 5000.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
        };

        this.context.Accounts.Add(account);
        await this.context.SaveChangesAsync();

        var result = await this.service.GetUserAccountAsync(userId2, accountId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserAccountAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        var accountId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.GetUserAccountAsync(Guid.Empty, accountId));
    }

    [Fact]
    public async Task GetUserAccountAsync_WithEmptyAccountId_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.GetUserAccountAsync(userId, Guid.Empty));
    }

    [Fact]
    public async Task GetUserAccountAsync_WithBothEmptyIds_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.GetUserAccountAsync(Guid.Empty, Guid.Empty));
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new AccountService(null!));
    }

    private void SeedTestData()
    {
        this.context.Database.EnsureDeleted();
        this.context.Database.EnsureCreated();
    }
}
