namespace HomeBanking.Domain;

/// <summary>
/// Represents a transaction (debit or credit) on an account.
/// Entity: encapsulates transaction details and validation rules.
/// </summary>
public class Transaction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Transaction"/> class.
    /// </summary>
    public Transaction()
    {
        // Required by EF Core for parameterless constructor
    }

    /// <summary>
    /// Gets the unique identifier for the transaction.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the account ID this transaction belongs to (foreign key).
    /// </summary>
    public required Guid AccountId { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount (must be > 0).
    /// The sign (debit/credit) is determined by the Type property.
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the date and time the transaction occurred.
    /// Cannot be in the future.
    /// </summary>
    public required DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the human-readable description of the transaction (e.g., "Grocery shopping", "Salary").
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

    /// <summary>
    /// Gets or sets the account this transaction belongs to (navigation property).
    /// </summary>
    public Account? Account { get; set; }

    /// <summary>
    /// Determines whether this transaction represents money affecting the balance.
    /// Both Debit and Credit transactions affect the balance, just in opposite directions.
    /// </summary>
    /// <returns>Always true; all transactions affect account balance.</returns>
    public bool AffectsBalance()
    {
        return !string.IsNullOrWhiteSpace(this.Type) &&
               (this.Type == DomainConstants.TransactionTypes.Debit || this.Type == DomainConstants.TransactionTypes.Credit);
    }

    /// <summary>
    /// Determines whether the transaction amount is valid (> 0 and within limits).
    /// </summary>
    /// <returns>True if amount is valid; otherwise false.</returns>
    public bool ValidateAmount()
    {
        return this.Amount > DomainConstants.TransactionLimits.MinTransactionAmount &&
               this.Amount <= DomainConstants.TransactionLimits.MaxTransactionAmount;
    }

    /// <summary>
    /// Determines whether the transaction date is in the past or present (not future).
    /// </summary>
    /// <returns>True if date is valid; otherwise false.</returns>
    public bool ValidateDate()
    {
        return this.Date <= DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the transaction category is valid (matches predefined categories).
    /// </summary>
    /// <returns>True if category is valid; otherwise false.</returns>
    public bool ValidateCategory()
    {
        return DomainConstants.TransactionCategories.AllCategories.Contains(this.Category);
    }

    /// <summary>
    /// Determines whether the transaction type is valid (Debit or Credit).
    /// </summary>
    /// <returns>True if type is valid; otherwise false.</returns>
    public bool ValidateType()
    {
        return DomainConstants.TransactionTypes.AllTypes.Contains(this.Type);
    }

    /// <summary>
    /// Validates all transaction invariants.
    /// </summary>
    /// <returns>True if all validations pass; otherwise false.</returns>
    public bool ValidateAll()
    {
        return this.ValidateAmount() &&
               this.ValidateDate() &&
               this.ValidateCategory() &&
               this.ValidateType() &&
               !string.IsNullOrWhiteSpace(this.Description) &&
               this.AccountId != Guid.Empty;
    }
}
