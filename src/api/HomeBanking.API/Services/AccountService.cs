namespace HomeBanking.API.Services;

using HomeBanking.API.Data;
using HomeBanking.API.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service for account-related business operations.
/// </summary>
public class AccountService : IAccountService
{
    private readonly HomeBankingContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    public AccountService(HomeBankingContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Retrieves all accounts for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A list of AccountDto objects for the user's accounts.</returns>
    /// <exception cref="ArgumentException">Thrown when userId is empty.</exception>
    public async Task<IEnumerable<AccountDto>> GetUserAccountsAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        var accounts = await this.context.Accounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                Name = a.Name,
                Balance = a.Balance,
                Currency = a.Currency,
                LastUpdated = a.LastUpdated,
            })
            .ToListAsync();

        return accounts;
    }

    /// <summary>
    /// Retrieves a specific account by ID for a given user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user (owner).</param>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <returns>The AccountDto if found; otherwise null.</returns>
    /// <exception cref="ArgumentException">Thrown when userId or accountId is empty.</exception>
    public async Task<AccountDto?> GetUserAccountAsync(Guid userId, Guid accountId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (accountId == Guid.Empty)
        {
            throw new ArgumentException("Account ID cannot be empty.", nameof(accountId));
        }

        var account = await this.context.Accounts
            .Where(a => a.UserId == userId && a.Id == accountId)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                Name = a.Name,
                Balance = a.Balance,
                Currency = a.Currency,
                LastUpdated = a.LastUpdated,
            })
            .FirstOrDefaultAsync();

        return account;
    }
}
