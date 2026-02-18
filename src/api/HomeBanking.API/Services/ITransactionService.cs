namespace HomeBanking.API.Services;

using HomeBanking.API.DTOs.Responses;

/// <summary>
/// Service interface for transaction-related business operations.
/// Handles retrieving and filtering transaction information.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Retrieves paginated transactions for a specific account belonging to a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user (account owner).</param>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="skip">Number of transactions to skip (pagination offset). Defaults to 0.</param>
    /// <param name="take">Maximum number of transactions to return (page size). Defaults to 10.</param>
    /// <param name="category">Optional category filter. If provided, only transactions matching this category are returned.</param>
    /// <returns>A task that returns a tuple containing the list of TransactionDto objects and the total count of transactions matching the criteria.</returns>
    /// <exception cref="ArgumentException">Thrown when userId or accountId is empty, or when skip/take are invalid.</exception>
    Task<(IEnumerable<TransactionDto> Transactions, int Total)> GetAccountTransactionsAsync(
        Guid userId,
        Guid accountId,
        int skip = 0,
        int take = 10,
        string? category = null);
}
