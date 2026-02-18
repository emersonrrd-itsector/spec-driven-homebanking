namespace HomeBanking.API.DTOs.Responses;

/// <summary>
/// Response DTO for account information including balance and last updated timestamp.
/// </summary>
public class AccountDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the account.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the friendly name of the account (e.g., "Checking", "Savings").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the current balance of the account.
    /// </summary>
    public required decimal Balance { get; set; }

    /// <summary>
    /// Gets or sets the currency code for the account (e.g., "USD", "EUR").
    /// </summary>
    public required string Currency { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last update to the account (UTC).
    /// </summary>
    public required DateTime LastUpdated { get; set; }
}
