namespace HomeBanking.API.DTOs.Responses;

/// <summary>
/// Response DTO for transaction information.
/// </summary>
public class TransactionDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the transaction.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the account ID this transaction belongs to.
    /// </summary>
    public required Guid AccountId { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount (always positive value).
    /// The sign (debit/credit) is determined by the Type property.
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the date and time the transaction occurred (UTC).
    /// </summary>
    public required DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the human-readable description of the transaction.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the category of the transaction.
    /// Valid values: Groceries, Utilities, Entertainment, Food &amp; Dining, Transport, Salary, Transfer, Other.
    /// </summary>
    public required string Category { get; set; }

    /// <summary>
    /// Gets or sets the transaction type: "Debit" (outgoing money) or "Credit" (incoming money).
    /// </summary>
    public required string Type { get; set; }
}
