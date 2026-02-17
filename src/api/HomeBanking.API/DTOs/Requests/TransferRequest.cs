namespace HomeBanking.API.DTOs.Requests;

/// <summary>
/// Request DTO for creating a transfer between two accounts.
/// </summary>
public class TransferRequest
{
    /// <summary>
    /// Gets or sets the source account ID.
    /// </summary>
    public required Guid FromAccountId { get; set; }

    /// <summary>
    /// Gets or sets the destination account ID.
    /// </summary>
    public required Guid ToAccountId { get; set; }

    /// <summary>
    /// Gets or sets the transfer amount (must be > 0.01).
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the optional description for the transfer.
    /// </summary>
    public string? Description { get; set; }
}
