namespace HomeBanking.Tests.Services;

using HomeBanking.API.Data;
using HomeBanking.API.Services;
using HomeBanking.Domain;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Unit tests for TransactionService.
/// Tests GetAccountTransactionsAsync() for pagination, filtering, and data retrieval.
/// </summary>
public class TransactionServiceTests
{
    private readonly HomeBankingContext context;
    private readonly TransactionService service;

    public TransactionServiceTests()
    {
        var options = new DbContextOptionsBuilder<HomeBankingContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        this.context = new HomeBankingContext(options);
        this.service = new TransactionService(this.context);
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_WithValidUserAndAccount_ReturnsTransactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hashed" };
        var account = new Account
        {
            Id = accountId,
            UserId = userId,
            Name = "Checking",
            Balance = 5000,
            Currency = "USD",
            AccountNumber = "ACC001",
            LastUpdated = DateTime.UtcNow,
        };
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Amount = 50.00m,
            Date = DateTime.UtcNow.AddDays(-1),
            Description = "Coffee",
            Category = "Food & Dining",
            Type = "Debit",
        };

        this.context.Users.Add(user);
        this.context.Accounts.Add(account);
        this.context.Transactions.Add(transaction);
        await this.context.SaveChangesAsync();

        // Act
        var (transactions, total) = await this.service.GetAccountTransactionsAsync(userId, accountId);

        // Assert
        Assert.Single(transactions);
        Assert.Equal(1, total);
        var returnedTx = transactions.First();
        Assert.Equal(transaction.Id, returnedTx.Id);
        Assert.Equal("Coffee", returnedTx.Description);
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_ReturnsAllTransactionFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hashed" };
        var account = new Account
        {
            Id = accountId,
            UserId = userId,
            Name = "Savings",
            Balance = 10000,
            Currency = "USD",
            AccountNumber = "ACC002",
            LastUpdated = DateTime.UtcNow,
        };
        var txDate = DateTime.UtcNow.AddDays(-5);
        var txId = Guid.NewGuid();
        var transaction = new Transaction
        {
            Id = txId,
            AccountId = accountId,
            Amount = 123.45m,
            Date = txDate,
            Description = "Groceries",
            Category = "Groceries",
            Type = "Debit",
        };

        this.context.Users.Add(user);
        this.context.Accounts.Add(account);
        this.context.Transactions.Add(transaction);
        await this.context.SaveChangesAsync();

        // Act
        var (transactions, _) = await this.service.GetAccountTransactionsAsync(userId, accountId);

        // Assert
        var tx = transactions.First();
        Assert.Equal(txId, tx.Id);
        Assert.Equal(accountId, tx.AccountId);
        Assert.Equal(123.45m, tx.Amount);
        Assert.Equal(txDate, tx.Date);
        Assert.Equal("Groceries", tx.Description);
        Assert.Equal("Groceries", tx.Category);
        Assert.Equal("Debit", tx.Type);
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_WithMultipleTransactions_ReturnsAll()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hashed" };
        var account = new Account
        {
            Id = accountId,
            UserId = userId,
            Name = "Checking",
            Balance = 5000,
            Currency = "USD",
            AccountNumber = "ACC003",
            LastUpdated = DateTime.UtcNow,
        };

        var transactions = Enumerable.Range(1, 15)
            .Select(i => new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 100m * i,
                Date = DateTime.UtcNow.AddDays(-i),
                Description = $"Transaction {i}",
                Category = i % 2 == 0 ? "Groceries" : "Utilities",
                Type = "Debit",
            })
            .ToList();

        this.context.Users.Add(user);
        this.context.Accounts.Add(account);
        this.context.Transactions.AddRange(transactions);
        await this.context.SaveChangesAsync();

        // Act
        var (result, total) = await this.service.GetAccountTransactionsAsync(userId, accountId, 0, 100);

        // Assert
        Assert.Equal(15, total);
        Assert.Equal(15, result.Count());
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_WithEmptyAccount_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hashed" };
        var account = new Account
        {
            Id = accountId,
            UserId = userId,
            Name = "Checking",
            Balance = 0,
            Currency = "USD",
            AccountNumber = "ACC004",
            LastUpdated = DateTime.UtcNow,
        };

        this.context.Users.Add(user);
        this.context.Accounts.Add(account);
        await this.context.SaveChangesAsync();

        // Act
        var (transactions, total) = await this.service.GetAccountTransactionsAsync(userId, accountId);

        // Assert
        Assert.Empty(transactions);
        Assert.Equal(0, total);
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_WithSkipAndTake_PaginatesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hashed" };
        var account = new Account
        {
            Id = accountId,
            UserId = userId,
            Name = "Checking",
            Balance = 5000,
            Currency = "USD",
            AccountNumber = "ACC005",
            LastUpdated = DateTime.UtcNow,
        };

        var transactions = Enumerable.Range(1, 30)
            .Select(i => new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 100m,
                Date = DateTime.UtcNow.AddDays(-i),
                Description = $"Tx {i}",
                Category = "Other",
                Type = "Debit",
            })
            .ToList();

        this.context.Users.Add(user);
        this.context.Accounts.Add(account);
        this.context.Transactions.AddRange(transactions);
        await this.context.SaveChangesAsync();

        // Act
        var (result1, total1) = await this.service.GetAccountTransactionsAsync(userId, accountId, 0, 10);
        var (result2, total2) = await this.service.GetAccountTransactionsAsync(userId, accountId, 10, 10);
        var (result3, total3) = await this.service.GetAccountTransactionsAsync(userId, accountId, 20, 10);

        // Assert
        Assert.Equal(10, result1.Count());
        Assert.Equal(10, result2.Count());
        Assert.Equal(10, result3.Count());
        Assert.Equal(30, total1);
        Assert.Equal(30, total2);
        Assert.Equal(30, total3);

        // Verify no overlap
        var ids1 = result1.Select(t => t.Id).ToHashSet();
        var ids2 = result2.Select(t => t.Id).ToHashSet();
        Assert.Empty(ids1.Intersect(ids2));
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_WithSkip0Take10_UsesDefaults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hashed" };
        var account = new Account
        {
            Id = accountId,
            UserId = userId,
            Name = "Checking",
            Balance = 5000,
            Currency = "USD",
            AccountNumber = "ACC006",
            LastUpdated = DateTime.UtcNow,
        };

        var transactions = Enumerable.Range(1, 15)
            .Select(i => new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 100m,
                Date = DateTime.UtcNow.AddDays(-i),
                Description = $"Tx {i}",
                Category = "Other",
                Type = "Debit",
            })
            .ToList();

        this.context.Users.Add(user);
        this.context.Accounts.Add(account);
        this.context.Transactions.AddRange(transactions);
        await this.context.SaveChangesAsync();

        // Act
        var (result, total) = await this.service.GetAccountTransactionsAsync(userId, accountId);

        // Assert
        Assert.Equal(10, result.Count());
        Assert.Equal(15, total);
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_ReturnsTransactionsOrderedByDateDescending()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hashed" };
        var account = new Account
        {
            Id = accountId,
            UserId = userId,
            Name = "Checking",
            Balance = 5000,
            Currency = "USD",
            AccountNumber = "ACC007",
            LastUpdated = DateTime.UtcNow,
        };

        var baseDate = DateTime.UtcNow;
        var transactions = new[]
        {
            new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 100m,
                Date = baseDate.AddDays(-3),
                Description = "Oldest",
                Category = "Other",
                Type = "Debit",
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 100m,
                Date = baseDate.AddDays(-1),
                Description = "Middle",
                Category = "Other",
                Type = "Debit",
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 100m,
                Date = baseDate,
                Description = "Newest",
                Category = "Other",
                Type = "Debit",
            },
        };

        this.context.Users.Add(user);
        this.context.Accounts.Add(account);
        this.context.Transactions.AddRange(transactions);
        await this.context.SaveChangesAsync();

        // Act
        var (result, _) = await this.service.GetAccountTransactionsAsync(userId, accountId, 0, 10);

        // Assert
        var resultList = result.ToList();
        Assert.Equal("Newest", resultList[0].Description);
        Assert.Equal("Middle", resultList[1].Description);
        Assert.Equal("Oldest", resultList[2].Description);
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_WithCategoryFilter_ReturnsOnlyMatchingTransactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hashed" };
        var account = new Account
        {
            Id = accountId,
            UserId = userId,
            Name = "Checking",
            Balance = 5000,
            Currency = "USD",
            AccountNumber = "ACC008",
            LastUpdated = DateTime.UtcNow,
        };

        var transactions = new[]
        {
            new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 50m,
                Date = DateTime.UtcNow.AddDays(-1),
                Description = "Grocery",
                Category = "Groceries",
                Type = "Debit",
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 100m,
                Date = DateTime.UtcNow,
                Description = "Electric",
                Category = "Utilities",
                Type = "Debit",
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 30m,
                Date = DateTime.UtcNow.AddDays(-2),
                Description = "Movie",
                Category = "Entertainment",
                Type = "Debit",
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 60m,
                Date = DateTime.UtcNow.AddDays(-3),
                Description = "Another Grocery",
                Category = "Groceries",
                Type = "Debit",
            },
        };

        this.context.Users.Add(user);
        this.context.Accounts.Add(account);
        this.context.Transactions.AddRange(transactions);
        await this.context.SaveChangesAsync();

        // Act
        var (result, total) = await this.service.GetAccountTransactionsAsync(userId, accountId, 0, 10, "Groceries");

        // Assert
        Assert.Equal(2, total);
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Equal("Groceries", t.Category));
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_WithNonmatchingCategory_ReturnsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hashed" };
        var account = new Account
        {
            Id = accountId,
            UserId = userId,
            Name = "Checking",
            Balance = 5000,
            Currency = "USD",
            AccountNumber = "ACC009",
            LastUpdated = DateTime.UtcNow,
        };

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Amount = 50m,
            Date = DateTime.UtcNow,
            Description = "Coffee",
            Category = "Food & Dining",
            Type = "Debit",
        };

        this.context.Users.Add(user);
        this.context.Accounts.Add(account);
        this.context.Transactions.Add(transaction);
        await this.context.SaveChangesAsync();

        // Act
        var (result, total) = await this.service.GetAccountTransactionsAsync(userId, accountId, 0, 10, "Salary");

        // Assert
        Assert.Empty(result);
        Assert.Equal(0, total);
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_WithCategoryFilterAndPagination_WorksTogether()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hashed" };
        var account = new Account
        {
            Id = accountId,
            UserId = userId,
            Name = "Checking",
            Balance = 5000,
            Currency = "USD",
            AccountNumber = "ACC010",
            LastUpdated = DateTime.UtcNow,
        };

        var transactions = Enumerable.Range(1, 25)
            .Select(i => new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 100m,
                Date = DateTime.UtcNow.AddDays(-i),
                Description = $"Grocery {i}",
                Category = "Groceries",
                Type = "Debit",
            })
            .ToList();

        this.context.Users.Add(user);
        this.context.Accounts.Add(account);
        this.context.Transactions.AddRange(transactions);
        await this.context.SaveChangesAsync();

        // Act
        var (page1, total1) = await this.service.GetAccountTransactionsAsync(userId, accountId, 0, 10, "Groceries");
        var (page2, total2) = await this.service.GetAccountTransactionsAsync(userId, accountId, 10, 10, "Groceries");

        // Assert
        Assert.Equal(10, page1.Count());
        Assert.Equal(10, page2.Count());
        Assert.Equal(25, total1);
        Assert.Equal(25, total2);
        Assert.Empty(page1.Select(t => t.Id).Intersect(page2.Select(t => t.Id)));
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.GetAccountTransactionsAsync(Guid.Empty, accountId));
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_WithEmptyAccountId_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.GetAccountTransactionsAsync(userId, Guid.Empty));
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_WithNegativeSkip_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.GetAccountTransactionsAsync(userId, accountId, -1));
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_WithZeroTake_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.GetAccountTransactionsAsync(userId, accountId, 0, 0));
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_WithTakeOver100_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.GetAccountTransactionsAsync(userId, accountId, 0, 101));
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_WithAccountFromDifferentUser_ReturnsEmpty()
    {
        // Arrange
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var user1 = new User { Id = user1Id, Email = "user1@example.com", PasswordHash = "hashed" };
        var user2 = new User { Id = user2Id, Email = "user2@example.com", PasswordHash = "hashed" };
        var account = new Account
        {
            Id = accountId,
            UserId = user2Id,  // Belongs to user2
            Name = "Checking",
            Balance = 5000,
            Currency = "USD",
            AccountNumber = "ACC011",
            LastUpdated = DateTime.UtcNow,
        };

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Amount = 100m,
            Date = DateTime.UtcNow,
            Description = "Test",
            Category = "Other",
            Type = "Debit",
        };

        this.context.Users.Add(user1);
        this.context.Users.Add(user2);
        this.context.Accounts.Add(account);
        this.context.Transactions.Add(transaction);
        await this.context.SaveChangesAsync();

        // Act - user1 tries to access user2's account
        var (result, total) = await this.service.GetAccountTransactionsAsync(user1Id, accountId);

        // Assert
        Assert.Empty(result);
        Assert.Equal(0, total);
    }

    [Fact]
    public async Task GetAccountTransactionsAsync_WithNonexistentAccount_ReturnsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonexistentAccountId = Guid.NewGuid();

        var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hashed" };
        this.context.Users.Add(user);
        await this.context.SaveChangesAsync();

        // Act
        var (result, total) = await this.service.GetAccountTransactionsAsync(userId, nonexistentAccountId);

        // Assert
        Assert.Empty(result);
        Assert.Equal(0, total);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new TransactionService(null!));
    }
}
