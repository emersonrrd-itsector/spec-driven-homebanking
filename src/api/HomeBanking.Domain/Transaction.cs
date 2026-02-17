namespace HomeBanking.Domain;

/// <summary>
/// Represents a transaction (debit or credit) on an account.
/// </summary>
public class Transaction
{
    /// <summary>
    /// Gets the unique identifier for the transaction.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the account ID this transaction belongs to (foreign key).
    /// </summary>
    public required Guid AccountId { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount (absolute value; sign determined by Type).
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the date and time the transaction occurred.
    /// </summary>
    public required DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the human-readable description of the transaction (e.g., "Grocery shopping").
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the category of the transaction (e.g., "Groceries", "Utilities", "Entertainment").
    /// </summary>
    public required string Category { get; set; }

    /// <summary>
    /// Gets or sets the transaction type: "Debit" (outgoing) or "Credit" (incoming).
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Gets or sets the account this transaction belongs to (navigation property).
    /// </summary>
    public Account? Account { get; set; }
}
