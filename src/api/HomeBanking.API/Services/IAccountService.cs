namespace HomeBanking.API.Services;

using HomeBanking.API.DTOs.Responses;

/// <summary>
/// Service interface for account-related business operations.
/// Handles retrieving account information and balance data.
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Retrieves all accounts for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task that returns a list of AccountDto objects for the user's accounts.</returns>
    /// <exception cref="ArgumentException">Thrown when userId is empty.</exception>
    Task<IEnumerable<AccountDto>> GetUserAccountsAsync(Guid userId);

    /// <summary>
    /// Retrieves a specific account by ID for a given user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user (owner).</param>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <returns>A task that returns the AccountDto if found; otherwise null.</returns>
    /// <exception cref="ArgumentException">Thrown when userId or accountId is empty.</exception>
    Task<AccountDto?> GetUserAccountAsync(Guid userId, Guid accountId);
}
