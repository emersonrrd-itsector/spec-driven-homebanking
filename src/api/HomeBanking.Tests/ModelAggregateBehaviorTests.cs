using HomeBanking.API.Data;
using HomeBanking.Domain;
using Microsoft.EntityFrameworkCore;

namespace HomeBanking.Tests;

/// <summary>
/// Unit tests for domain model aggregates and value objects.
/// Validates business rules, validation logic, and aggregate behavior.
/// </summary>
public class ModelAggregateBehaviorTests
{
    /// <summary>
    /// Test: User can validate email format correctly.
    /// </summary>
    [Fact]
    public void User_ValidateEmailFormat_WithValidEmail_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash123",
            IsActive = true,
        };

        // Act
        var isValid = user.ValidateEmailFormat();

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Test: User validation fails for invalid email format.
    /// </summary>
    [Fact]
    public void User_ValidateEmailFormat_WithInvalidEmail_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "not-an-email",
            PasswordHash = "hash123",
            IsActive = true,
        };

        // Act
        var isValid = user.ValidateEmailFormat();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Test: User can verify correct password.
    /// </summary>
    [Fact]
    public void User_PasswordHashMatches_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        const string plainPassword = "demo123";
        var passwordHash = PasswordHash.CreateFromPlainText(plainPassword);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = passwordHash.Hash,
            IsActive = true,
        };

        // Act
        var matches = user.PasswordHashMatches(plainPassword);

        // Assert
        Assert.True(matches);
    }

    /// <summary>
    /// Test: User password verification fails for incorrect password.
    /// </summary>
    [Fact]
    public void User_PasswordHashMatches_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        const string plainPassword = "demo123";
        const string wrongPassword = "wrongpassword";
        var passwordHash = PasswordHash.CreateFromPlainText(plainPassword);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = passwordHash.Hash,
            IsActive = true,
        };

        // Act
        var matches = user.PasswordHashMatches(wrongPassword);

        // Assert
        Assert.False(matches);
    }

    /// <summary>
    /// Test: Account can withdraw with sufficient balance.
    /// </summary>
    [Fact]
    public void Account_CanWithdraw_WithSufficientBalance_ReturnsTrue()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Checking",
            Balance = 1000m,
            Currency = "USD",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        // Act
        var canWithdraw = account.CanWithdraw(500m);

        // Assert
        Assert.True(canWithdraw);
    }

    /// <summary>
    /// Test: Account cannot withdraw with insufficient balance.
    /// </summary>
    [Fact]
    public void Account_CanWithdraw_WithInsufficientBalance_ReturnsFalse()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Checking",
            Balance = 100m,
            Currency = "USD",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        // Act
        var canWithdraw = account.CanWithdraw(500m);

        // Assert
        Assert.False(canWithdraw);
    }

    /// <summary>
    /// Test: Account can withdraw entire balance.
    /// </summary>
    [Fact]
    public void Account_CanWithdraw_WithEntireBalance_ReturnsTrue()
    {
        // Arrange
        const decimal balance = 1000m;
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Checking",
            Balance = balance,
            Currency = "USD",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        // Act
        var canWithdraw = account.CanWithdraw(balance);

        // Assert
        Assert.True(canWithdraw);
    }

    /// <summary>
    /// Test: Account cannot withdraw zero or negative amount.
    /// </summary>
    /// <param name="amount">The withdrawal amount to test.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Account_CanWithdraw_WithInvalidAmount_ReturnsFalse(decimal amount)
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Checking",
            Balance = 1000m,
            Currency = "USD",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        // Act
        var canWithdraw = account.CanWithdraw(amount);

        // Assert
        Assert.False(canWithdraw);
    }

    /// <summary>
    /// Test: Account cannot withdraw when inactive.
    /// </summary>
    [Fact]
    public void Account_CanWithdraw_WhenInactive_ReturnsFalse()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Checking",
            Balance = 1000m,
            Currency = "USD",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = false, // Inactive account
        };

        // Act
        var canWithdraw = account.CanWithdraw(500m);

        // Assert
        Assert.False(canWithdraw);
    }

    /// <summary>
    /// Test: Withdraw successfully updates balance and creates transaction.
    /// </summary>
    [Fact]
    public void Account_Withdraw_UpdatesBalanceAndCreatesTransaction()
    {
        // Arrange
        const decimal withdrawAmount = 250m;
        const decimal initialBalance = 1000m;
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Checking",
            Balance = initialBalance,
            Currency = "USD",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        // Act
        var transaction = account.Withdraw(withdrawAmount, "Grocery shopping", DomainConstants.TransactionCategories.Groceries);

        // Assert
        Assert.Equal(initialBalance - withdrawAmount, account.Balance);
        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal(account.Id, transaction.AccountId);
        Assert.Equal(withdrawAmount, transaction.Amount);
        Assert.Equal(DomainConstants.TransactionTypes.Debit, transaction.Type);
        Assert.Single(account.Transactions);
    }

    /// <summary>
    /// Test: Withdraw throws exception when insufficient balance.
    /// </summary>
    [Fact]
    public void Account_Withdraw_WithInsufficientBalance_ThrowsException()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Checking",
            Balance = 100m,
            Currency = "USD",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => account.Withdraw(500m, "Test", DomainConstants.TransactionCategories.Other));
    }

    /// <summary>
    /// Test: Deposit successfully updates balance and creates transaction.
    /// </summary>
    [Fact]
    public void Account_Deposit_UpdatesBalanceAndCreatesTransaction()
    {
        // Arrange
        const decimal depositAmount = 500m;
        const decimal initialBalance = 1000m;
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Checking",
            Balance = initialBalance,
            Currency = "USD",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        // Act
        var transaction = account.Deposit(depositAmount, "Salary", DomainConstants.TransactionCategories.Salary);

        // Assert
        Assert.Equal(initialBalance + depositAmount, account.Balance);
        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal(account.Id, transaction.AccountId);
        Assert.Equal(depositAmount, transaction.Amount);
        Assert.Equal(DomainConstants.TransactionTypes.Credit, transaction.Type);
        Assert.Single(account.Transactions);
    }

    /// <summary>
    /// Test: Account can transfer to different account with sufficient balance.
    /// </summary>
    [Fact]
    public void Account_CanTransferTo_WithSufficientBalance_ReturnsTrue()
    {
        // Arrange
        var sourceAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Checking",
            Balance = 1000m,
            Currency = "USD",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var targetAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Savings",
            Balance = 500m,
            Currency = "USD",
            AccountNumber = "654321",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        // Act
        var canTransfer = sourceAccount.CanTransferTo(targetAccount, 300m);

        // Assert
        Assert.True(canTransfer);
    }

    /// <summary>
    /// Test: Account cannot transfer to itself.
    /// </summary>
    [Fact]
    public void Account_CanTransferTo_ToSameAccount_ReturnsFalse()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Checking",
            Balance = 1000m,
            Currency = "USD",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        // Act
        var canTransfer = account.CanTransferTo(account, 300m);

        // Assert
        Assert.False(canTransfer);
    }

    /// <summary>
    /// Test: Account cannot transfer to different currency account.
    /// </summary>
    [Fact]
    public void Account_CanTransferTo_ToDifferentCurrencyAccount_ReturnsFalse()
    {
        // Arrange
        var sourceAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "USD Checking",
            Balance = 1000m,
            Currency = "USD",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var targetAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "EUR Savings",
            Balance = 500m,
            Currency = "EUR",
            AccountNumber = "654321",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        // Act
        var canTransfer = sourceAccount.CanTransferTo(targetAccount, 300m);

        // Assert
        Assert.False(canTransfer);
    }

    /// <summary>
    /// Test: Transfer successfully updates both accounts and creates transactions.
    /// </summary>
    [Fact]
    public void Account_TransferTo_UpdatesBothAccountsAndCreatesTransactions()
    {
        // Arrange
        const decimal transferAmount = 250m;
        const decimal sourceInitialBalance = 1000m;
        const decimal targetInitialBalance = 500m;

        var sourceAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Checking",
            Balance = sourceInitialBalance,
            Currency = "USD",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var targetAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Savings",
            Balance = targetInitialBalance,
            Currency = "USD",
            AccountNumber = "654321",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        // Act
        var (sourceTransaction, targetTransaction) = sourceAccount.TransferTo(targetAccount, transferAmount, "Personal transfer");

        // Assert - Balance updates
        Assert.Equal(sourceInitialBalance - transferAmount, sourceAccount.Balance);
        Assert.Equal(targetInitialBalance + transferAmount, targetAccount.Balance);

        // Assert - Source transaction
        Assert.Equal(sourceAccount.Id, sourceTransaction.AccountId);
        Assert.Equal(transferAmount, sourceTransaction.Amount);
        Assert.Equal(DomainConstants.TransactionTypes.Debit, sourceTransaction.Type);
        Assert.Equal(DomainConstants.TransactionCategories.Transfer, sourceTransaction.Category);

        // Assert - Target transaction
        Assert.Equal(targetAccount.Id, targetTransaction.AccountId);
        Assert.Equal(transferAmount, targetTransaction.Amount);
        Assert.Equal(DomainConstants.TransactionTypes.Credit, targetTransaction.Type);
        Assert.Equal(DomainConstants.TransactionCategories.Transfer, targetTransaction.Category);

        // Assert - Transactions added to collections
        Assert.Single(sourceAccount.Transactions);
        Assert.Single(targetAccount.Transactions);
    }

    /// <summary>
    /// Test: Transaction amount validation - must be greater than zero.
    /// </summary>
    [Fact]
    public void Transaction_ValidateAmount_WithInvalidAmount_ReturnsFalse()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Amount = 0,  // Invalid: zero amount
            Date = DateTime.UtcNow,
            Description = "Test",
            Category = DomainConstants.TransactionCategories.Other,
            Type = DomainConstants.TransactionTypes.Debit,
        };

        // Act
        var isValid = transaction.ValidateAmount();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Test: Transaction date validation - cannot be in the future.
    /// </summary>
    [Fact]
    public void Transaction_ValidateDate_WithFutureDate_ReturnsFalse()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Amount = 100m,
            Date = DateTime.UtcNow.AddDays(1),  // Future date
            Description = "Test",
            Category = DomainConstants.TransactionCategories.Other,
            Type = DomainConstants.TransactionTypes.Debit,
        };

        // Act
        var isValid = transaction.ValidateDate();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Test: Transaction date validation - can be in the past or present.
    /// </summary>
    [Fact]
    public void Transaction_ValidateDate_WithPastOrCurrentDate_ReturnsTrue()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Amount = 100m,
            Date = DateTime.UtcNow.AddSeconds(-5),  // Past
            Description = "Test",
            Category = DomainConstants.TransactionCategories.Other,
            Type = DomainConstants.TransactionTypes.Debit,
        };

        // Act
        var isValid = transaction.ValidateDate();

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Test: Transaction affects balance for both debit and credit.
    /// </summary>
    /// <param name="transactionType">The transaction type (Debit or Credit) to test.</param>
    [Theory]
    [InlineData(DomainConstants.TransactionTypes.Debit)]
    [InlineData(DomainConstants.TransactionTypes.Credit)]
    public void Transaction_AffectsBalance_WithValidType_ReturnsTrue(string transactionType)
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Amount = 100m,
            Date = DateTime.UtcNow,
            Description = "Test",
            Category = DomainConstants.TransactionCategories.Other,
            Type = transactionType,
        };

        // Act
        var affects = transaction.AffectsBalance();

        // Assert
        Assert.True(affects);
    }

    /// <summary>
    /// Test: Account balance cannot be negative after multiple operations.
    /// </summary>
    [Fact]
    public void Account_ValidateBalance_AfterMultipleOperations_IsValid()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Checking",
            Balance = 1000m,
            Currency = "USD",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        // Act - Perform multiple operations
        account.Withdraw(300m, "Purchase", DomainConstants.TransactionCategories.Groceries);
        account.Deposit(500m, "Salary", DomainConstants.TransactionCategories.Salary);
        account.Withdraw(600m, "Bills", DomainConstants.TransactionCategories.Utilities);

        // Assert - Balance should be valid (not negative)
        var isValid = account.ValidateBalance();
        Assert.True(isValid);
        Assert.Equal(600m, account.Balance);
    }

    /// <summary>
    /// Test: User can get account by ID from their collection.
    /// </summary>
    [Fact]
    public void User_GetAccountById_WithExistingAccount_ReturnsAccount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var account = new Account
        {
            Id = accountId,
            UserId = userId,
            Name = "Checking",
            Balance = 1000m,
            Currency = "USD",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hash123",
            IsActive = true,
            Accounts = new List<Account> { account },
        };

        // Act
        var retrievedAccount = user.GetAccountById(accountId);

        // Assert
        Assert.NotNull(retrievedAccount);
        Assert.Equal(accountId, retrievedAccount.Id);
    }

    /// <summary>
    /// Test: Account currency validation.
    /// </summary>
    [Fact]
    public void Account_ValidateCurrency_WithValidCurrency_ReturnsTrue()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Checking",
            Balance = 1000m,
            Currency = "USD",  // Valid 3-char code
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        // Act
        var isValid = account.ValidateCurrency();

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Test: Money value object prevents negative amounts.
    /// </summary>
    [Fact]
    public void Money_Constructor_WithNegativeAmount_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Money(-100m, "USD"));
    }

    /// <summary>
    /// Test: Money value object validates currency format.
    /// </summary>
    [Fact]
    public void Money_Constructor_WithInvalidCurrency_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Money(100m, "US"));  // Only 2 chars
    }

    /// <summary>
    /// Test: Email value object validates format.
    /// </summary>
    [Fact]
    public void Email_Constructor_WithInvalidFormat_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Email("not-an-email"));
    }
}
