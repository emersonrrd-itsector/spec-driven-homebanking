namespace HomeBanking.Tests.Services;

using HomeBanking.API.Data;
using HomeBanking.API.Services;
using HomeBanking.Domain;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Unit tests for TransferService.
/// Tests transfer validation, transaction creation, and balance updates.
/// </summary>
public class TransferServiceTests
{
    private readonly HomeBankingContext context;
    private readonly TransferService service;

    public TransferServiceTests()
    {
        var options = new DbContextOptionsBuilder<HomeBankingContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        this.context = new HomeBankingContext(options);
        this.service = new TransferService(this.context);
        this.SeedTestData();
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithValidTransfer_SucceedsAndCreatesTransactions()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var transferAmount = 100.00m;

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            transferAmount,
            "Test transfer",
            userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(fromAccountId, result.FromAccountId);
        Assert.Equal(toAccountId, result.ToAccountId);
        Assert.Equal(transferAmount, result.Amount);
        Assert.Equal(400.00m, result.FromAccountNewBalance);
        Assert.Equal(1100.00m, result.ToAccountNewBalance);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithValidTransfer_UpdatesBalancesInDatabase()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var transferAmount = 250.00m;

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 2000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            transferAmount,
            "Test transfer",
            userId);

        var updatedFromAccount = this.context.Accounts.First(a => a.Id == fromAccountId);
        var updatedToAccount = this.context.Accounts.First(a => a.Id == toAccountId);

        Assert.Equal(750.00m, updatedFromAccount.Balance);
        Assert.Equal(2250.00m, updatedToAccount.Balance);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithValidTransfer_CreatesDebitTransactionInSourceAccount()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var transferAmount = 100.00m;

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            transferAmount,
            "Test transfer",
            userId);

        var debitTransaction = this.context.Transactions.First(
            t => t.AccountId == fromAccountId && t.Type == DomainConstants.TransactionTypes.Debit);

        Assert.NotNull(debitTransaction);
        Assert.Equal(fromAccountId, debitTransaction.AccountId);
        Assert.Equal(transferAmount, debitTransaction.Amount);
        Assert.Equal(DomainConstants.TransactionTypes.Debit, debitTransaction.Type);
        Assert.Equal(DomainConstants.TransactionCategories.Transfer, debitTransaction.Category);
        Assert.Contains("Transfer to", debitTransaction.Description);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithValidTransfer_CreatesCreditTransactionInDestinationAccount()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var transferAmount = 100.00m;

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            transferAmount,
            "Test transfer",
            userId);

        var creditTransaction = this.context.Transactions.First(
            t => t.AccountId == toAccountId && t.Type == DomainConstants.TransactionTypes.Credit);

        Assert.NotNull(creditTransaction);
        Assert.Equal(toAccountId, creditTransaction.AccountId);
        Assert.Equal(transferAmount, creditTransaction.Amount);
        Assert.Equal(DomainConstants.TransactionTypes.Credit, creditTransaction.Type);
        Assert.Equal(DomainConstants.TransactionCategories.Transfer, creditTransaction.Category);
        Assert.Contains("Transfer from", creditTransaction.Description);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithValidTransfer_TransactionsAreAtomic()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var transferAmount = 100.00m;

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            transferAmount,
            "Test transfer",
            userId);

        var transactions = this.context.Transactions
            .Where(t => (t.AccountId == fromAccountId && t.Type == DomainConstants.TransactionTypes.Debit) ||
                        (t.AccountId == toAccountId && t.Type == DomainConstants.TransactionTypes.Credit))
            .ToList();

        Assert.Equal(2, transactions.Count);
        Assert.True(transactions[0].Date <= transactions[1].Date);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithMinimumValidAmount_Succeeds()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var transferAmount = 0.01m;

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 1.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            transferAmount,
            null,
            userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(0.99m, result.FromAccountNewBalance);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithExactBalance_Succeeds()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var transferAmount = 500.00m;

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            transferAmount,
            null,
            userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(0.00m, result.FromAccountNewBalance);
        Assert.Equal(1500.00m, result.ToAccountNewBalance);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithInsufficientBalance_ReturnsFail()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var transferAmount = 1000.00m;

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            transferAmount,
            "Test transfer",
            userId);

        Assert.False(result.IsSuccess);
        Assert.Equal("InsufficientBalance", result.ErrorCode);
        Assert.Contains("exceeds", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithInsufficientBalance_ReturnsErrorDetails()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var transferAmount = 1000.00m;
        var availableBalance = 500.00m;

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = availableBalance,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            transferAmount,
            "Test transfer",
            userId);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorDetails);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithInsufficientBalance_DoesNotModifyBalances()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var transferAmount = 1000.00m;

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            transferAmount,
            "Test transfer",
            userId);

        var updatedFromAccount = this.context.Accounts.First(a => a.Id == fromAccountId);
        var updatedToAccount = this.context.Accounts.First(a => a.Id == toAccountId);

        Assert.Equal(500.00m, updatedFromAccount.Balance);
        Assert.Equal(1000.00m, updatedToAccount.Balance);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithNonexistentSourceAccount_ReturnsFail()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.Add(toAccount);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            100.00m,
            "Test transfer",
            userId);

        Assert.False(result.IsSuccess);
        Assert.Equal("AccountNotFound", result.ErrorCode);
        Assert.Contains("Source account", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithNonexistentDestinationAccount_ReturnsFail()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.Add(fromAccount);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            100.00m,
            "Test transfer",
            userId);

        Assert.False(result.IsSuccess);
        Assert.Equal("AccountNotFound", result.ErrorCode);
        Assert.Contains("Destination account", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithSameSourceAndDestination_ReturnsFail()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var account = new Account
        {
            Id = accountId,
            UserId = userId,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.Add(account);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            accountId,
            accountId,
            100.00m,
            "Test transfer",
            userId);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidTransfer", result.ErrorCode);
        Assert.Contains("same account", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithInactiveSourceAccount_ReturnsFail()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = false,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            100.00m,
            "Test transfer",
            userId);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidTransfer", result.ErrorCode);
        Assert.Contains("inactive", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithInactiveDestinationAccount_ReturnsFail()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = false,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            100.00m,
            "Test transfer",
            userId);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidTransfer", result.ErrorCode);
        Assert.Contains("inactive", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithDifferentCurrencies_ReturnsFail()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "EUR",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            100.00m,
            "Test transfer",
            userId);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidTransfer", result.ErrorCode);
        Assert.Contains("currency", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WhenUserDoesNotOwnSourceAccount_ReturnsFail()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId1,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId2,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            100.00m,
            "Test transfer",
            userId2);

        Assert.False(result.IsSuccess);
        Assert.Equal("Unauthorized", result.ErrorCode);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WhenUserDoesNotOwnDestinationAccount_ReturnsFail()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId1,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId2,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            100.00m,
            "Test transfer",
            userId1);

        Assert.False(result.IsSuccess);
        Assert.Equal("Unauthorized", result.ErrorCode);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithEmptyFromAccountId_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.ExecuteTransferAsync(
                Guid.Empty,
                toAccountId,
                100.00m,
                "Test",
                userId));
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithEmptyToAccountId_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.ExecuteTransferAsync(
                fromAccountId,
                Guid.Empty,
                100.00m,
                "Test",
                userId));
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.ExecuteTransferAsync(
                fromAccountId,
                toAccountId,
                100.00m,
                "Test",
                Guid.Empty));
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithNegativeAmount_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.ExecuteTransferAsync(
                fromAccountId,
                toAccountId,
                -100.00m,
                "Test",
                userId));
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithZeroAmount_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.ExecuteTransferAsync(
                fromAccountId,
                toAccountId,
                0.00m,
                "Test",
                userId));
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithAmountTooSmall_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentException>(
            () => this.service.ExecuteTransferAsync(
                fromAccountId,
                toAccountId,
                0.001m,
                "Test",
                userId));
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new TransferService(null!));
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithVeryLargeAmount_Succeeds()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var transferAmount = 99999.99m;

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 100000.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            transferAmount,
            "Large transfer",
            userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(0.01m, result.FromAccountNewBalance);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithNullDescription_Succeeds()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            100.00m,
            null,
            userId);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteTransferAsync_WithEmptyDescription_Succeeds()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        var result = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            100.00m,
            string.Empty,
            userId);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteTransferAsync_MultipleTransfers_AreRecordedCorrectly()
    {
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var fromAccount = new Account
        {
            Id = fromAccountId,
            UserId = userId,
            Name = "Checking",
            Balance = 1000.00m,
            Currency = "USD",
            AccountNumber = "1001",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var toAccount = new Account
        {
            Id = toAccountId,
            UserId = userId,
            Name = "Savings",
            Balance = 500.00m,
            Currency = "USD",
            AccountNumber = "1002",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        this.context.Accounts.AddRange(fromAccount, toAccount);
        await this.context.SaveChangesAsync();

        var result1 = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            100.00m,
            "Transfer 1",
            userId);

        var result2 = await this.service.ExecuteTransferAsync(
            fromAccountId,
            toAccountId,
            150.00m,
            "Transfer 2",
            userId);

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.Equal(750.00m, result2.FromAccountNewBalance);
        Assert.Equal(750.00m, result2.ToAccountNewBalance);

        var allTransactions = this.context.Transactions.ToList();
        Assert.Equal(4, allTransactions.Count);
    }

    private void SeedTestData()
    {
        this.context.Database.EnsureDeleted();
        this.context.Database.EnsureCreated();
    }
}
