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

    /// <summary>
    /// Test: Email value object equality.
    /// </summary>
    [Fact]
    public void Email_Equals_WithSameValue_ReturnsTrue()
    {
        // Act & Assert
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");
        Assert.Equal(email1, email2);
    }

    /// <summary>
    /// Test: Email value object case-insensitive equality.
    /// </summary>
    [Fact]
    public void Email_Equals_CaseInsensitive_ReturnsTrue()
    {
        // Act & Assert
        var email1 = new Email("test@example.com");
        var email2 = new Email("TEST@EXAMPLE.COM");
        Assert.Equal(email1, email2);
    }

    /// <summary>
    /// Test: Email value object hash code consistency.
    /// </summary>
    [Fact]
    public void Email_GetHashCode_WithSameValue_ReturnsSameHashCode()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");

        // Act & Assert
        Assert.Equal(email1.GetHashCode(), email2.GetHashCode());
    }

    /// <summary>
    /// Test: Email value object string representation.
    /// </summary>
    [Fact]
    public void Email_ToString_ReturnsEmailValue()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act & Assert
        Assert.Equal("test@example.com", email.ToString());
    }

    /// <summary>
    /// Test: Email value object with null throws exception.
    /// </summary>
    [Fact]
    public void Email_Constructor_WithNull_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(null!));
    }

    /// <summary>
    /// Test: Email value object with empty string throws exception.
    /// </summary>
    [Fact]
    public void Email_Constructor_WithEmpty_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(string.Empty));
    }

    /// <summary>
    /// Test: Email IsValidFormat with valid emails.
    /// </summary>
    /// <param name="email">Valid email to test.</param>
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("a@b.c")]
    public void Email_IsValidFormat_WithValidEmails_ReturnsTrue(string email)
    {
        // Act & Assert
        Assert.True(Email.IsValidFormat(email));
    }

    /// <summary>
    /// Test: Email IsValidFormat with invalid emails.
    /// </summary>
    /// <param name="email">Invalid email to test.</param>
    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test @example.com")]
    public void Email_IsValidFormat_WithInvalidEmails_ReturnsFalse(string email)
    {
        // Act & Assert
        Assert.False(Email.IsValidFormat(email));
    }

    /// <summary>
    /// Test: Money value object equality.
    /// </summary>
    [Fact]
    public void Money_Equals_WithSameAmountAndCurrency_ReturnsTrue()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");

        // Act & Assert
        Assert.Equal(money1, money2);
    }

    /// <summary>
    /// Test: Money value object inequality with different amounts.
    /// </summary>
    [Fact]
    public void Money_Equals_WithDifferentAmounts_ReturnsFalse()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(200m, "USD");

        // Act & Assert
        Assert.NotEqual(money1, money2);
    }

    /// <summary>
    /// Test: Money value object inequality with different currencies.
    /// </summary>
    [Fact]
    public void Money_Equals_WithDifferentCurrencies_ReturnsFalse()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "EUR");

        // Act & Assert
        Assert.NotEqual(money1, money2);
    }

    /// <summary>
    /// Test: Money value object hash code consistency.
    /// </summary>
    [Fact]
    public void Money_GetHashCode_WithSameValue_ReturnsSameHashCode()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");

        // Act & Assert
        Assert.Equal(money1.GetHashCode(), money2.GetHashCode());
    }

    /// <summary>
    /// Test: Money value object string representation.
    /// </summary>
    [Fact]
    public void Money_ToString_FormatsCorrectly()
    {
        // Arrange
        var money = new Money(100m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        Assert.Contains("USD", result);
        Assert.Contains("100", result);
    }

    /// <summary>
    /// Test: Money Create factory method.
    /// </summary>
    [Fact]
    public void Money_Create_WithValidValues_ReturnsValidMoney()
    {
        // Act
        var money = Money.Create(500m, "GBP");

        // Assert
        Assert.Equal(500m, money.Amount);
        Assert.Equal("GBP", money.Currency);
    }

    /// <summary>
    /// Test: Money currency normalization to uppercase.
    /// </summary>
    [Fact]
    public void Money_Constructor_NormalizesCurrencyToUppercase()
    {
        // Act
        var money = new Money(100m, "usd");

        // Assert
        Assert.Equal("USD", money.Currency);
    }

    /// <summary>
    /// Test: Money with null currency throws exception.
    /// </summary>
    [Fact]
    public void Money_Constructor_WithNullCurrency_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Money(100m, null!));
    }

    /// <summary>
    /// Test: Account cannot deposit zero amount.
    /// </summary>
    [Fact]
    public void Account_Deposit_WithZeroAmount_ThrowsException()
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

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => account.Deposit(0m, "Test", DomainConstants.TransactionCategories.Other));
    }

    /// <summary>
    /// Test: Account cannot deposit to inactive account.
    /// </summary>
    [Fact]
    public void Account_Deposit_ToInactiveAccount_ThrowsException()
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
            IsActive = false,
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => account.Deposit(500m, "Test", DomainConstants.TransactionCategories.Other));
    }

    /// <summary>
    /// Test: Account cannot transfer with null target account.
    /// </summary>
    [Fact]
    public void Account_CanTransferTo_WithNullTargetAccount_ReturnsFalse()
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
        var canTransfer = account.CanTransferTo(null!, 500m);

        // Assert
        Assert.False(canTransfer);
    }

    /// <summary>
    /// Test: Account cannot transfer to inactive source account.
    /// </summary>
    [Fact]
    public void Account_CanTransferTo_WithInactiveSourceAccount_ReturnsFalse()
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
            IsActive = false,
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
        Assert.False(canTransfer);
    }

    /// <summary>
    /// Test: Account cannot transfer to inactive target account.
    /// </summary>
    [Fact]
    public void Account_CanTransferTo_WithInactiveTargetAccount_ReturnsFalse()
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
            IsActive = false,
        };

        // Act
        var canTransfer = sourceAccount.CanTransferTo(targetAccount, 300m);

        // Assert
        Assert.False(canTransfer);
    }

    /// <summary>
    /// Test: Account cannot transfer with zero or negative amount.
    /// </summary>
    /// <param name="amount">The transfer amount to test.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Account_CanTransferTo_WithInvalidAmount_ReturnsFalse(decimal amount)
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
        var canTransfer = sourceAccount.CanTransferTo(targetAccount, amount);

        // Assert
        Assert.False(canTransfer);
    }

    /// <summary>
    /// Test: Account cannot transfer with insufficient balance.
    /// </summary>
    [Fact]
    public void Account_CanTransferTo_WithInsufficientBalance_ReturnsFalse()
    {
        // Arrange
        var sourceAccount = new Account
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
        var canTransfer = sourceAccount.CanTransferTo(targetAccount, 500m);

        // Assert
        Assert.False(canTransfer);
    }

    /// <summary>
    /// Test: Transaction cannot be validated with invalid type.
    /// </summary>
    [Fact]
    public void Transaction_ValidateType_WithInvalidType_ReturnsFalse()
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
            Type = "InvalidType",
        };

        // Act
        var isValid = transaction.ValidateType();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Test: Transaction cannot be validated with invalid category.
    /// </summary>
    [Fact]
    public void Transaction_ValidateCategory_WithInvalidCategory_ReturnsFalse()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Amount = 100m,
            Date = DateTime.UtcNow,
            Description = "Test",
            Category = "InvalidCategory",
            Type = DomainConstants.TransactionTypes.Debit,
        };

        // Act
        var isValid = transaction.ValidateCategory();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Test: Transaction ValidateAll passes with valid data.
    /// </summary>
    [Fact]
    public void Transaction_ValidateAll_WithValidData_ReturnsTrue()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Amount = 100m,
            Date = DateTime.UtcNow,
            Description = "Valid transaction",
            Category = DomainConstants.TransactionCategories.Groceries,
            Type = DomainConstants.TransactionTypes.Debit,
        };

        // Act
        var isValid = transaction.ValidateAll();

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Test: Transaction ValidateAll fails with empty description.
    /// </summary>
    [Fact]
    public void Transaction_ValidateAll_WithEmptyDescription_ReturnsFalse()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Amount = 100m,
            Date = DateTime.UtcNow,
            Description = string.Empty,
            Category = DomainConstants.TransactionCategories.Other,
            Type = DomainConstants.TransactionTypes.Debit,
        };

        // Act
        var isValid = transaction.ValidateAll();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Test: Transaction ValidateAll fails with empty account ID.
    /// </summary>
    [Fact]
    public void Transaction_ValidateAll_WithEmptyAccountId_ReturnsFalse()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = Guid.Empty,
            Amount = 100m,
            Date = DateTime.UtcNow,
            Description = "Test",
            Category = DomainConstants.TransactionCategories.Other,
            Type = DomainConstants.TransactionTypes.Debit,
        };

        // Act
        var isValid = transaction.ValidateAll();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Test: User GetAccountById returns null for non-existent account.
    /// </summary>
    [Fact]
    public void User_GetAccountById_WithNonexistentAccountId_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hash123",
            IsActive = true,
            Accounts = new List<Account>(),
        };

        // Act
        var account = user.GetAccountById(Guid.NewGuid());

        // Assert
        Assert.Null(account);
    }

    /// <summary>
    /// Test: User GetActiveAccounts returns only active accounts.
    /// </summary>
    [Fact]
    public void User_GetActiveAccounts_ReturnsOnlyActiveAccounts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activeAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Active",
            Balance = 1000m,
            Currency = "USD",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        var inactiveAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Inactive",
            Balance = 500m,
            Currency = "USD",
            AccountNumber = "654321",
            LastUpdated = DateTime.UtcNow,
            IsActive = false,
        };

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hash123",
            IsActive = true,
            Accounts = new List<Account> { activeAccount, inactiveAccount },
        };

        // Act
        var activeAccounts = user.GetActiveAccounts().ToList();

        // Assert
        Assert.Single(activeAccounts);
        Assert.Equal("Active", activeAccounts[0].Name);
    }

    /// <summary>
    /// Test: User GetActiveAccounts returns empty list when no active accounts.
    /// </summary>
    [Fact]
    public void User_GetActiveAccounts_WithNoActiveAccounts_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var inactiveAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Inactive",
            Balance = 500m,
            Currency = "USD",
            AccountNumber = "654321",
            LastUpdated = DateTime.UtcNow,
            IsActive = false,
        };

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hash123",
            IsActive = true,
            Accounts = new List<Account> { inactiveAccount },
        };

        // Act
        var activeAccounts = user.GetActiveAccounts().ToList();

        // Assert
        Assert.Empty(activeAccounts);
    }

    /// <summary>
    /// Test: User OwnsAccount returns true for owned account.
    /// </summary>
    [Fact]
    public void User_OwnsAccount_WithOwnedAccount_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new Account
        {
            Id = Guid.NewGuid(),
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
        };

        // Act
        var owns = user.OwnsAccount(account);

        // Assert
        Assert.True(owns);
    }

    /// <summary>
    /// Test: User OwnsAccount returns false for non-owned account.
    /// </summary>
    [Fact]
    public void User_OwnsAccount_WithNonOwnedAccount_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId,
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
        };

        // Act
        var owns = user.OwnsAccount(account);

        // Assert
        Assert.False(owns);
    }

    /// <summary>
    /// Test: PasswordHash Create factory method.
    /// </summary>
    [Fact]
    public void PasswordHash_Create_WithValidPassword_ReturnsPasswordHash()
    {
        // Act
        var hash = PasswordHash.Create("password123");

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash.Hash);
    }

    /// <summary>
    /// Test: PasswordHash Create throws for null password.
    /// </summary>
    [Fact]
    public void PasswordHash_Create_WithNullPassword_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => PasswordHash.Create(null!));
    }

    /// <summary>
    /// Test: PasswordHash Create throws for empty password.
    /// </summary>
    [Fact]
    public void PasswordHash_Create_WithEmptyPassword_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => PasswordHash.Create(string.Empty));
    }

    /// <summary>
    /// Test: PasswordHash CreateFromPlainText is alias for Create.
    /// </summary>
    [Fact]
    public void PasswordHash_CreateFromPlainText_WorksCorrectly()
    {
        // Act
        var hash = PasswordHash.CreateFromPlainText("test123");

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash.Hash);
    }

    /// <summary>
    /// Test: PasswordHash Matches returns false for null password.
    /// </summary>
    [Fact]
    public void PasswordHash_Matches_WithNullPassword_ReturnsFalse()
    {
        // Arrange
        var hash = PasswordHash.Create("password123");

        // Act
        var matches = hash.Matches(null!);

        // Assert
        Assert.False(matches);
    }

    /// <summary>
    /// Test: PasswordHash Matches returns false for empty password.
    /// </summary>
    [Fact]
    public void PasswordHash_Matches_WithEmptyPassword_ReturnsFalse()
    {
        // Arrange
        var hash = PasswordHash.Create("password123");

        // Act
        var matches = hash.Matches(string.Empty);

        // Assert
        Assert.False(matches);
    }

    /// <summary>
    /// Test: PasswordHash ToString returns hash value.
    /// </summary>
    [Fact]
    public void PasswordHash_ToString_ReturnsHashValue()
    {
        // Arrange
        var password = "test123";
        var hash = PasswordHash.Create(password);

        // Act
        var result = hash.ToString();

        // Assert
        Assert.Equal(hash.Hash, result);
    }

    /// <summary>
    /// Test: Account ValidateAccountNumber returns false for null.
    /// </summary>
    [Fact]
    public void Account_ValidateAccountNumber_WithNull_ReturnsFalse()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Checking",
            Balance = 1000m,
            Currency = "USD",
            AccountNumber = null!,
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        // Act
        var isValid = account.ValidateAccountNumber();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Test: Account ValidateCurrency returns false for invalid length.
    /// </summary>
    [Fact]
    public void Account_ValidateCurrency_WithInvalidLength_ReturnsFalse()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Checking",
            Balance = 1000m,
            Currency = "US",
            AccountNumber = "123456",
            LastUpdated = DateTime.UtcNow,
            IsActive = true,
        };

        // Act
        var isValid = account.ValidateCurrency();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Test: User PasswordHashMatches with null password hash.
    /// </summary>
    [Fact]
    public void User_PasswordHashMatches_WithNullPasswordHash_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = null!,
            IsActive = true,
        };

        // Act
        var matches = user.PasswordHashMatches("password");

        // Assert
        Assert.False(matches);
    }

    /// <summary>
    /// Test: User PasswordHashMatches with exception handling.
    /// </summary>
    [Fact]
    public void User_PasswordHashMatches_WithInvalidHashFormat_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "not-a-valid-hash",
            IsActive = true,
        };

        // Act
        var matches = user.PasswordHashMatches("password123");

        // Assert
        Assert.False(matches);
    }

    /// <summary>
    /// Test: Account Withdraw with invalid category uses default.
    /// </summary>
    [Fact]
    public void Account_Withdraw_WithInvalidCategory_UsesDefaultCategory()
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
        var transaction = account.Withdraw(100m, "Test", "InvalidCategory");

        // Assert
        Assert.Equal(DomainConstants.TransactionCategories.Other, transaction.Category);
    }

    /// <summary>
    /// Test: Account Deposit with invalid category uses default.
    /// </summary>
    [Fact]
    public void Account_Deposit_WithInvalidCategory_UsesDefaultCategory()
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
        var transaction = account.Deposit(100m, "Test", "InvalidCategory");

        // Assert
        Assert.Equal(DomainConstants.TransactionCategories.Other, transaction.Category);
    }

    /// <summary>
    /// Test: DomainConstants CurrencyCodes AllCurrencies property.
    /// </summary>
    [Fact]
    public void DomainConstants_CurrencyCodes_AllCurrencies_ContainsExpectedCurrencies()
    {
        // Act & Assert
        Assert.NotEmpty(DomainConstants.CurrencyCodes.AllCurrencies);
        Assert.Contains("USD", DomainConstants.CurrencyCodes.AllCurrencies);
        Assert.Contains("EUR", DomainConstants.CurrencyCodes.AllCurrencies);
        Assert.Contains("GBP", DomainConstants.CurrencyCodes.AllCurrencies);
    }

    /// <summary>
    /// Test: DomainConstants TransactionTypes AllTypes property.
    /// </summary>
    [Fact]
    public void DomainConstants_TransactionTypes_AllTypes_ContainsExpectedTypes()
    {
        // Act & Assert
        Assert.NotEmpty(DomainConstants.TransactionTypes.AllTypes);
        Assert.Contains("Debit", DomainConstants.TransactionTypes.AllTypes);
        Assert.Contains("Credit", DomainConstants.TransactionTypes.AllTypes);
    }

    /// <summary>
    /// Test: DomainConstants TransactionLimits constants.
    /// </summary>
    [Fact]
    public void DomainConstants_TransactionLimits_HasValidConstants()
    {
        // Act & Assert
        Assert.Equal(0.01m, DomainConstants.TransactionLimits.MinTransactionAmount);
        Assert.Equal(100_000m, DomainConstants.TransactionLimits.MaxTransactionAmount);
        Assert.Equal(0m, DomainConstants.TransactionLimits.MinBalance);
    }
}
