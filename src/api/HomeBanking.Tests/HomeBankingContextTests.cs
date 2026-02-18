using HomeBanking.API.Data;
using HomeBanking.Domain;
using Microsoft.EntityFrameworkCore;

namespace HomeBanking.Tests;

/// <summary>
/// Unit tests for the HomeBankingContext DbContext.
/// Validates initialization, seed data, and relationships.
/// </summary>
public class HomeBankingContextTests
{
    /// <summary>
    /// Test: DbContext initializes without errors.
    /// </summary>
    [Fact]
    public void Initialize_ContextCreatedSuccessfully()
    {
        // Arrange & Act
        using var context = this.CreateTestContext();

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.Users);
        Assert.NotNull(context.Accounts);
        Assert.NotNull(context.Transactions);
    }

    /// <summary>
    /// Test: Seed data is populated with exactly one user.
    /// </summary>
    [Fact]
    public void Seed_UserCreatedInDatabase()
    {
        // Arrange
        using var context = this.CreateTestContext();

        // Act
        var users = context.Users.ToList();

        // Assert
        Assert.Single(users);
        Assert.Equal("admin@homebank.local", users[0].Email);
    }

    /// <summary>
    /// Test: Seed data creates exactly 2 accounts.
    /// </summary>
    [Fact]
    public void Seed_AccountsCreatedForUser()
    {
        // Arrange
        using var context = this.CreateTestContext();

        // Act
        var accounts = context.Accounts.ToList();

        // Assert
        Assert.Equal(2, accounts.Count);
        Assert.Contains(accounts, a => a.Name == "Checking");
        Assert.Contains(accounts, a => a.Name == "Savings");
    }

    /// <summary>
    /// Test: Seed data creates ~15 transactions per account.
    /// </summary>
    [Fact]
    public void Seed_TransactionsCreatedPerAccount()
    {
        // Arrange
        using var context = this.CreateTestContext();

        // Act
        var transactions = context.Transactions.ToList();

        // Assert
        Assert.NotEmpty(transactions);
        Assert.Equal(30, transactions.Count); // ~15 per account (2 accounts)

        var account1Transactions = transactions.Where(t => t.AccountId == Guid.Parse("550e8400-e29b-41d4-a716-446655440001")).Count();
        var account2Transactions = transactions.Where(t => t.AccountId == Guid.Parse("550e8400-e29b-41d4-a716-446655440002")).Count();

        Assert.Equal(15, account1Transactions);
        Assert.Equal(15, account2Transactions);
    }

    /// <summary>
    /// Test: Account balances are seeded correctly.
    /// </summary>
    [Fact]
    public void Seed_AccountBalancesSetCorrectly()
    {
        // Arrange
        using var context = this.CreateTestContext();

        // Act
        var checking = context.Accounts.FirstOrDefault(a => a.Name == "Checking");
        var savings = context.Accounts.FirstOrDefault(a => a.Name == "Savings");

        // Assert
        Assert.NotNull(checking);
        Assert.NotNull(savings);
        Assert.Equal(5000.00m, checking.Balance);
        Assert.Equal(15000.00m, savings.Balance);
    }

    /// <summary>
    /// Test: User→Accounts relationship is correctly configured.
    /// </summary>
    [Fact]
    public void Relationships_UserHasAccounts()
    {
        // Arrange
        using var context = this.CreateTestContext();

        // Act
        var user = context.Users.Include(u => u.Accounts).FirstOrDefault();

        // Assert
        Assert.NotNull(user);
        Assert.Equal(2, user.Accounts.Count);
    }

    /// <summary>
    /// Test: Account→Transactions relationship is correctly configured.
    /// </summary>
    [Fact]
    public void Relationships_AccountHasTransactions()
    {
        // Arrange
        using var context = this.CreateTestContext();

        // Act
        var account = context.Accounts.Include(a => a.Transactions).FirstOrDefault();

        // Assert
        Assert.NotNull(account);
        Assert.Equal(15, account.Transactions.Count);
    }

    /// <summary>
    /// Test: Transaction categories are populated with expected values.
    /// </summary>
    [Fact]
    public void Seed_TransactionCategoriesPopulated()
    {
        // Arrange
        using var context = this.CreateTestContext();

        // Act
        var transactions = context.Transactions.ToList();
        var categories = transactions.Select(t => t.Category).Distinct().ToList();

        // Assert
        Assert.NotEmpty(categories);
        Assert.Contains("Salary", categories);
        Assert.Contains("Groceries", categories);
        Assert.Contains("Utilities", categories);
        Assert.Contains("Entertainment", categories);
        Assert.Contains("Food & Dining", categories);
        Assert.Contains("Transport", categories);
    }

    /// <summary>
    /// Test: Transaction types are either "Debit" or "Credit".
    /// </summary>
    [Fact]
    public void Seed_TransactionTypesValid()
    {
        // Arrange
        using var context = this.CreateTestContext();

        // Act
        var transactions = context.Transactions.ToList();
        var invalidTypes = transactions.Where(t => t.Type != "Debit" && t.Type != "Credit").ToList();

        // Assert
        Assert.Empty(invalidTypes);
    }

    /// <summary>
    /// Test: All transactions have valid amounts (greater than 0).
    /// </summary>
    [Fact]
    public void Seed_TransactionAmountsValid()
    {
        // Arrange
        using var context = this.CreateTestContext();

        // Act
        var transactions = context.Transactions.ToList();
        var invalidTransactions = transactions.Where(t => t.Amount <= 0).ToList();

        // Assert
        Assert.Empty(invalidTransactions);
    }

    /// <summary>
    /// Test: Password hash is stored (not plain text).
    /// </summary>
    [Fact]
    public void Seed_PasswordHashedNotPlainText()
    {
        // Arrange
        using var context = this.CreateTestContext();

        // Act
        var user = context.Users.FirstOrDefault();

        // Assert
        Assert.NotNull(user);
        Assert.NotEmpty(user.PasswordHash);
        Assert.NotEqual("demo123", user.PasswordHash); // Should be hashed, not plain text
        Assert.True(user.PasswordHash.Length > 10); // SHA256 hash is long
    }

    /// <summary>
    /// Test: Account last updated timestamps are set.
    /// </summary>
    [Fact]
    public void Seed_LastUpdatedTimestampsSet()
    {
        // Arrange
        using var context = this.CreateTestContext();

        // Act
        var accounts = context.Accounts.ToList();

        // Assert
        Assert.All(accounts, account =>
        {
            Assert.NotEqual(DateTime.MinValue, account.LastUpdated);
            Assert.True(account.LastUpdated <= DateTime.UtcNow);
        });
    }

    /// <summary>
    /// Test: Add a new user and verify it's persisted.
    /// </summary>
    [Fact]
    public void AddUser_UserAddedToDatabase()
    {
        // Arrange
        using var context = this.CreateTestContext();
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@homebank.local",
            PasswordHash = "testhash123",
        };

        // Act
        context.Users.Add(newUser);
        context.SaveChanges();

        // Assert
        var addedUser = context.Users.FirstOrDefault(u => u.Email == "test@homebank.local");
        Assert.NotNull(addedUser);
        Assert.Equal(newUser.Id, addedUser.Id);
    }

    /// <summary>
    /// Test: Add a new account and verify it's associated with a user.
    /// </summary>
    [Fact]
    public void AddAccount_AccountAssociatedWithUser()
    {
        // Arrange
        using var context = this.CreateTestContext();
        var user = context.Users.FirstOrDefault();
        Assert.NotNull(user);

        var newAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = "Test Account",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "9999999999999",
            LastUpdated = DateTime.UtcNow,
        };

        // Act
        context.Accounts.Add(newAccount);
        context.SaveChanges();

        // Assert
        var addedAccount = context.Accounts.FirstOrDefault(a => a.Name == "Test Account");
        Assert.NotNull(addedAccount);
        Assert.Equal(user.Id, addedAccount.UserId);
    }

    /// <summary>
    /// Test: Add a transaction and verify it's associated with an account.
    /// </summary>
    [Fact]
    public void AddTransaction_TransactionAssociatedWithAccount()
    {
        // Arrange
        using var context = this.CreateTestContext();
        var account = context.Accounts.FirstOrDefault();
        Assert.NotNull(account);

        var newTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Amount = 100.00m,
            Date = DateTime.UtcNow,
            Description = "Test transaction",
            Category = "Other",
            Type = "Debit",
        };

        // Act
        context.Transactions.Add(newTransaction);
        context.SaveChanges();

        // Assert
        var addedTransaction = context.Transactions.FirstOrDefault(t => t.Description == "Test transaction");
        Assert.NotNull(addedTransaction);
        Assert.Equal(account.Id, addedTransaction.AccountId);
    }

    /// <summary>
    /// Test: Cascade delete removes transactions when account is deleted.
    /// </summary>
    [Fact]
    public void OnDelete_CascadeDeletesTransactions()
    {
        // Arrange
        using var context = this.CreateTestContext();
        var account = context.Accounts.Include(a => a.Transactions).FirstOrDefault();
        Assert.NotNull(account);
        var initialTransactionCount = account.Transactions.Count;

        // Act
        context.Accounts.Remove(account);
        context.SaveChanges();

        // Assert
        var transactionsForDeletedAccount = context.Transactions
            .Where(t => t.AccountId == account.Id)
            .ToList();

        Assert.Empty(transactionsForDeletedAccount);
        var remainingAccounts = context.Accounts.Count();
        Assert.Equal(1, remainingAccounts); // Should have 1 account left (originally 2)
    }

    /// <summary>
    /// Creates a new in-memory DbContext instance for testing.
    /// </summary>
    /// <returns>A new HomeBankingContext configured with InMemory database.</returns>
    private HomeBankingContext CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<HomeBankingContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        var context = new HomeBankingContext(options);
        context.Database.EnsureCreated();
        context.SeedIfEmpty();
        return context;
    }
}
