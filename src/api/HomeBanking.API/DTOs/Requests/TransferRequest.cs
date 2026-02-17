namespace HomeBanking.API.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request DTO for creating a transfer between two accounts.
/// </summary>
public class TransferRequest
{
    /// <summary>
    /// Gets or sets the source account ID.
    /// </summary>
    [Required(ErrorMessage = "From account ID is required.")]
    public required Guid FromAccountId { get; set; }

    /// <summary>
    /// Gets or sets the destination account ID.
    /// </summary>
    [Required(ErrorMessage = "To account ID is required.")]
    public required Guid ToAccountId { get; set; }

    /// <summary>
    /// Gets or sets the transfer amount (must be > 0.01).
    /// </summary>
    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Transfer amount must be greater than 0.01.")]
    public required decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the optional description for the transfer.
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }
}
