namespace HomeBanking.API.DTOs.Responses;

/// <summary>
/// Response DTO for successful transfer operations.
/// Includes transfer details and updated account balances.
/// </summary>
public class TransferResponse
{
    /// <summary>
    /// Gets or sets the unique identifier of the transfer.
    /// </summary>
    public required string TransferId { get; set; }

    /// <summary>
    /// Gets or sets the status of the transfer (e.g., "completed").
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Gets or sets the source account details with new balance.
    /// </summary>
    public required TransferAccountResponse FromAccount { get; set; }

    /// <summary>
    /// Gets or sets the destination account details with new balance.
    /// </summary>
    public required TransferAccountResponse ToAccount { get; set; }

    /// <summary>
    /// Gets or sets the transferred amount.
    /// </summary>
    public required decimal Amount { get; set; }
}
