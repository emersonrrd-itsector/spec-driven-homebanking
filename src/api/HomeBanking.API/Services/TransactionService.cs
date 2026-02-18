namespace HomeBanking.API.Services;

using HomeBanking.API.Data;
using HomeBanking.API.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service for transaction-related business operations.
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly HomeBankingContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    public TransactionService(HomeBankingContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Retrieves paginated transactions for a specific account belonging to a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user (account owner).</param>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="skip">Number of transactions to skip (pagination offset). Defaults to 0.</param>
    /// <param name="take">Maximum number of transactions to return (page size). Defaults to 10.</param>
    /// <param name="category">Optional category filter. If provided, only transactions matching this category are returned.</param>
    /// <returns>A tuple containing the list of TransactionDto objects and the total count of transactions matching the criteria.</returns>
    /// <exception cref="ArgumentException">Thrown when userId or accountId is empty, or when skip/take are invalid.</exception>
    public async Task<(IEnumerable<TransactionDto> Transactions, int Total)> GetAccountTransactionsAsync(
        Guid userId,
        Guid accountId,
        int skip = 0,
        int take = 10,
        string? category = null)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (accountId == Guid.Empty)
        {
            throw new ArgumentException("Account ID cannot be empty.", nameof(accountId));
        }

        if (skip < 0)
        {
            throw new ArgumentException("Skip value cannot be negative.", nameof(skip));
        }

        if (take < 1 || take > 100)
        {
            throw new ArgumentException("Take value must be between 1 and 100.", nameof(take));
        }

        // Verify the account belongs to the user
        var accountExists = await this.context.Accounts
            .AnyAsync(a => a.Id == accountId && a.UserId == userId);

        if (!accountExists)
        {
            // Return empty result if account doesn't belong to user or doesn't exist
            return (new List<TransactionDto>(), 0);
        }

        // Build base query
        var query = this.context.Transactions
            .Where(t => t.AccountId == accountId);

        // Apply category filter if provided
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(t => t.Category == category);
        }

        // Get total count before pagination
        var total = await query.CountAsync();

        // Apply pagination and select
        var transactions = await query
            .OrderByDescending(t => t.Date)
            .Skip(skip)
            .Take(take)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                AccountId = t.AccountId,
                Amount = t.Amount,
                Date = t.Date,
                Description = t.Description,
                Category = t.Category,
                Type = t.Type,
            })
            .ToListAsync();

        return (transactions, total);
    }
}
