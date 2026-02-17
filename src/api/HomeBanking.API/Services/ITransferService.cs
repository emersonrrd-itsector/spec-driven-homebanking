namespace HomeBanking.API.Services;

using HomeBanking.API.DTOs.Responses;

/// <summary>
/// Service interface for transfer operations.
/// Handles inter-account transfers with validation and transaction recording.
/// </summary>
public interface ITransferService
{
    /// <summary>
    /// Executes a transfer from one account to another.
    /// Creates debit transaction in source account and credit transaction in destination account.
    /// Updates balances atomically.
    /// </summary>
    /// <param name="fromAccountId">The source account ID.</param>
    /// <param name="toAccountId">The destination account ID.</param>
    /// <param name="amount">The transfer amount (must be > 0).</param>
    /// <param name="description">Optional description for the transfer.</param>
    /// <param name="userId">The user ID (for authorization check).</param>
    /// <returns>
    /// A task that returns a TransferResult object containing transfer details,
    /// or error information if transfer fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    Task<TransferResult> ExecuteTransferAsync(
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount,
        string? description,
        Guid userId);
}
