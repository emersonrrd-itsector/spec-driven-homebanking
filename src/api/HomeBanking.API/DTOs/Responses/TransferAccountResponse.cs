namespace HomeBanking.API.DTOs.Responses;

/// <summary>
/// Nested DTO representing account details in a transfer response.
/// </summary>
public class TransferAccountResponse
{
    /// <summary>
    /// Gets or sets the unique identifier of the account.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the new balance of the account after the transfer.
    /// </summary>
    public required decimal NewBalance { get; set; }
}
