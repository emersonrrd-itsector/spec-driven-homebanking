namespace HomeBanking.API.DTOs.Responses;

/// <summary>
/// Response DTO for transfer operations.
/// Indicates success with new balances or failure with error details.
/// </summary>
public class TransferResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the transfer was successful.
    /// </summary>
    public required bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the error code (e.g., "InsufficientBalance", "AccountNotFound", "InvalidTransfer").
    /// Null if transfer is successful.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the error message describing why the transfer failed.
    /// Null if transfer is successful.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets optional error details (e.g., available vs. requested balance).
    /// Null if transfer is successful.
    /// </summary>
    public object? ErrorDetails { get; set; }

    /// <summary>
    /// Gets or sets the source account ID (populated on success).
    /// </summary>
    public Guid? FromAccountId { get; set; }

    /// <summary>
    /// Gets or sets the destination account ID (populated on success).
    /// </summary>
    public Guid? ToAccountId { get; set; }

    /// <summary>
    /// Gets or sets the transfer amount (populated on success).
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Gets or sets the new balance of the source account (populated on success).
    /// </summary>
    public decimal? FromAccountNewBalance { get; set; }

    /// <summary>
    /// Gets or sets the new balance of the destination account (populated on success).
    /// </summary>
    public decimal? ToAccountNewBalance { get; set; }

    /// <summary>
    /// Creates a successful transfer result.
    /// </summary>
    /// <param name="fromAccountId">The source account ID.</param>
    /// <param name="toAccountId">The destination account ID.</param>
    /// <param name="amount">The transferred amount.</param>
    /// <param name="fromAccountNewBalance">The new balance of the source account.</param>
    /// <param name="toAccountNewBalance">The new balance of the destination account.</param>
    /// <returns>A TransferResult indicating success.</returns>
    public static TransferResult Success(
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount,
        decimal fromAccountNewBalance,
        decimal toAccountNewBalance)
    {
        return new TransferResult
        {
            IsSuccess = true,
            ErrorCode = null,
            ErrorMessage = null,
            ErrorDetails = null,
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amount,
            FromAccountNewBalance = fromAccountNewBalance,
            ToAccountNewBalance = toAccountNewBalance,
        };
    }

    /// <summary>
    /// Creates a failed transfer result.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorDetails">Optional error details.</param>
    /// <returns>A TransferResult indicating failure.</returns>
    public static TransferResult Failure(
        string errorCode,
        string errorMessage,
        object? errorDetails = null)
    {
        return new TransferResult
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            ErrorDetails = errorDetails,
            FromAccountId = null,
            ToAccountId = null,
            Amount = null,
            FromAccountNewBalance = null,
            ToAccountNewBalance = null,
        };
    }
}
