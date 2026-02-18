namespace HomeBanking.Domain;

/// <summary>
/// Represents a bank account owned by a user.
/// Aggregate root: encapsulates account identity, balance management, and transaction lifecycle.
/// </summary>
public class Account
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Account"/> class.
    /// </summary>
    public Account()
    {
        // Required by EF Core for parameterless constructor
    }

    /// <summary>
    /// Gets the unique identifier for the account.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the user ID (owner of the account, foreign key).
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the friendly name of the account (e.g., "Checking", "Savings").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the current balance of the account in the specified currency.
    /// Must be >= 0 (no negative balances allowed).
    /// </summary>
    public required decimal Balance { get; set; }

    /// <summary>
    /// Gets or sets the currency code for the account (e.g., "USD", "EUR").
    /// Must be exactly 3 uppercase characters.
    /// </summary>
    public required string Currency { get; set; }

    /// <summary>
    /// Gets or sets the account number (unique identifier used for transfers).
    /// Must be unique across all accounts.
    /// </summary>
    public required string AccountNumber { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last update to the account (balance change, transfer, etc.).
    /// </summary>
    public required DateTime LastUpdated { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the account is active.
    /// Default: true. Inactive accounts cannot perform transactions.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the user who owns this account (navigation property).
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets the collection of transactions associated with this account.
    /// </summary>
    public ICollection<Transaction> Transactions { get; init; } = new List<Transaction>();

    /// <summary>
    /// Determines whether this account can withdraw a specified amount.
    /// Validates that balance is sufficient and amount is positive.
    /// </summary>
    /// <param name="amount">The amount to withdraw (must be > 0).</param>
    /// <returns>True if withdrawal is permitted; otherwise false.</returns>
    public bool CanWithdraw(decimal amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (!this.IsActive)
        {
            return false;
        }

        if (this.Balance < amount)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether this account can transfer a specified amount to another account.
    /// Validates that balance is sufficient, amount is positive, and accounts are different and active.
    /// </summary>
    /// <param name="toAccount">The destination account for the transfer.</param>
    /// <param name="amount">The amount to transfer (must be > 0).</param>
    /// <returns>True if transfer is permitted; otherwise false.</returns>
    public bool CanTransferTo(Account toAccount, decimal amount)
    {
        if (toAccount == null)
        {
            return false;
        }

        // Cannot transfer to the same account
        if (this.Id == toAccount.Id)
        {
            return false;
        }

        // Both accounts must be active
        if (!this.IsActive || !toAccount.IsActive)
        {
            return false;
        }

        // Must have sufficient balance
        if (this.Balance < amount)
        {
            return false;
        }

        // Amount must be positive
        if (amount <= 0)
        {
            return false;
        }

        // Currencies must match (transfers are in same currency)
        if (this.Currency != toAccount.Currency)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Withdraws an amount from this account, creating a debit transaction.
    /// </summary>
    /// <param name="amount">The amount to withdraw (must be > 0).</param>
    /// <param name="description">Description of the withdrawal.</param>
    /// <param name="category">Transaction category.</param>
    /// <returns>The created debit transaction.</returns>
    /// <exception cref="InvalidOperationException">Thrown if withdrawal is not permitted.</exception>
    public Transaction Withdraw(decimal amount, string description, string category)
    {
        if (!this.CanWithdraw(amount))
        {
            throw new InvalidOperationException($"Cannot withdraw {amount:C} from account {this.AccountNumber}. Insufficient funds or inactive account.");
        }

        if (!DomainConstants.TransactionCategories.AllCategories.Contains(category))
        {
            category = DomainConstants.TransactionCategories.Other;
        }

        this.Balance -= amount;
        this.LastUpdated = DateTime.UtcNow;

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = this.Id,
            Amount = amount,
            Date = DateTime.UtcNow,
            Description = description,
            Category = category,
            Type = DomainConstants.TransactionTypes.Debit,
            Account = this,
        };

        this.Transactions.Add(transaction);
        return transaction;
    }

    /// <summary>
    /// Deposits an amount into this account, creating a credit transaction.
    /// </summary>
    /// <param name="amount">The amount to deposit (must be > 0).</param>
    /// <param name="description">Description of the deposit.</param>
    /// <param name="category">Transaction category.</param>
    /// <returns>The created credit transaction.</returns>
    /// <exception cref="InvalidOperationException">Thrown if account is not active or amount is invalid.</exception>
    public Transaction Deposit(decimal amount, string description, string category)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException("Deposit amount must be greater than zero.");
        }

        if (!this.IsActive)
        {
            throw new InvalidOperationException("Cannot deposit to an inactive account.");
        }

        if (!DomainConstants.TransactionCategories.AllCategories.Contains(category))
        {
            category = DomainConstants.TransactionCategories.Other;
        }

        this.Balance += amount;
        this.LastUpdated = DateTime.UtcNow;

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = this.Id,
            Amount = amount,
            Date = DateTime.UtcNow,
            Description = description,
            Category = category,
            Type = DomainConstants.TransactionTypes.Credit,
            Account = this,
        };

        this.Transactions.Add(transaction);
        return transaction;
    }

    /// <summary>
    /// Transfers an amount from this account to another account.
    /// Creates matching transactions: debit in source, credit in destination.
    /// </summary>
    /// <param name="toAccount">The destination account.</param>
    /// <param name="amount">The amount to transfer (must be > 0).</param>
    /// <param name="description">Description of the transfer.</param>
    /// <returns>A tuple of (SourceTransaction, DestinationTransaction).</returns>
    /// <exception cref="InvalidOperationException">Thrown if transfer is not permitted.</exception>
    public (Transaction SourceTransaction, Transaction DestinationTransaction) TransferTo(Account toAccount, decimal amount, string description)
    {
        if (!this.CanTransferTo(toAccount, amount))
        {
            throw new InvalidOperationException($"Transfer of {amount:C} from account {this.AccountNumber} to {toAccount.AccountNumber} is not permitted.");
        }

        // Create outgoing debit transaction
        var outgoingDescription = $"Transfer to {toAccount.AccountNumber}";
        this.Balance -= amount;
        this.LastUpdated = DateTime.UtcNow;

        var sourceTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = this.Id,
            Amount = amount,
            Date = DateTime.UtcNow,
            Description = outgoingDescription,
            Category = DomainConstants.TransactionCategories.Transfer,
            Type = DomainConstants.TransactionTypes.Debit,
            Account = this,
        };
        this.Transactions.Add(sourceTransaction);

        // Create incoming credit transaction
        var incomingDescription = $"Transfer from {this.AccountNumber}";
        toAccount.Balance += amount;
        toAccount.LastUpdated = DateTime.UtcNow;

        var destinationTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = toAccount.Id,
            Amount = amount,
            Date = DateTime.UtcNow,
            Description = incomingDescription,
            Category = DomainConstants.TransactionCategories.Transfer,
            Type = DomainConstants.TransactionTypes.Credit,
            Account = toAccount,
        };
        toAccount.Transactions.Add(destinationTransaction);

        return (sourceTransaction, destinationTransaction);
    }

    /// <summary>
    /// Validates that the account's balance is not negative (invariant).
    /// </summary>
    /// <returns>True if balance is valid; otherwise false.</returns>
    public bool ValidateBalance()
    {
        return this.Balance >= DomainConstants.TransactionLimits.MinBalance;
    }

    /// <summary>
    /// Validates that the account number is valid (not empty).
    /// </summary>
    /// <returns>True if account number is valid; otherwise false.</returns>
    public bool ValidateAccountNumber()
    {
        return !string.IsNullOrWhiteSpace(this.AccountNumber);
    }

    /// <summary>
    /// Validates that the currency code is exactly 3 characters.
    /// </summary>
    /// <returns>True if currency code is valid; otherwise false.</returns>
    public bool ValidateCurrency()
    {
        return !string.IsNullOrWhiteSpace(this.Currency) && this.Currency.Length == 3;
    }
}
