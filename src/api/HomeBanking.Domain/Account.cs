namespace HomeBanking.Domain;

/// <summary>
/// Represents a bank account owned by a user.
/// </summary>
public class Account
{
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
    /// </summary>
    public required decimal Balance { get; set; }

    /// <summary>
    /// Gets or sets the currency code for the account (e.g., "USD", "EUR").
    /// </summary>
    public required string Currency { get; set; }

    /// <summary>
    /// Gets or sets the account number (unique identifier used for transfers).
    /// </summary>
    public required string AccountNumber { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last update to the account (balance change, transfer, etc.).
    /// </summary>
    public required DateTime LastUpdated { get; set; }

    /// <summary>
    /// Gets or sets the user who owns this account (navigation property).
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets the collection of transactions associated with this account.
    /// </summary>
    public ICollection<Transaction> Transactions { get; init; } = new List<Transaction>();
}
