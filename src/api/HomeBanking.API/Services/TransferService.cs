namespace HomeBanking.API.Services;

using HomeBanking.API.Data;
using HomeBanking.API.DTOs.Responses;
using HomeBanking.Domain;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service for transfer-related business operations.
/// Handles validation, transaction recording, and atomic balance updates for transfers.
/// </summary>
public class TransferService : ITransferService
{
    private readonly HomeBankingContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransferService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    public TransferService(HomeBankingContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Executes a transfer from one account to another.
    /// Validates: sufficient balance, both accounts exist, source ≠ destination, user owns both accounts.
    /// Creates transactions atomically in both accounts and updates balances.
    /// </summary>
    /// <param name="fromAccountId">The source account ID.</param>
    /// <param name="toAccountId">The destination account ID.</param>
    /// <param name="amount">The transfer amount (must be > 0.01).</param>
    /// <param name="description">Optional description for the transfer.</param>
    /// <param name="userId">The user ID (for authorization check).</param>
    /// <returns>TransferResult with success or error information.</returns>
    /// <exception cref="ArgumentException">Thrown when IDs are empty or amount is invalid.</exception>
    public async Task<TransferResult> ExecuteTransferAsync(
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount,
        string? description,
        Guid userId)
    {
        if (fromAccountId == Guid.Empty)
        {
            throw new ArgumentException("Source account ID cannot be empty.", nameof(fromAccountId));
        }

        if (toAccountId == Guid.Empty)
        {
            throw new ArgumentException("Destination account ID cannot be empty.", nameof(toAccountId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (amount < DomainConstants.TransactionLimits.MinTransactionAmount)
        {
            throw new ArgumentException(
                $"Transfer amount must be at least {DomainConstants.TransactionLimits.MinTransactionAmount:C}.",
                nameof(amount));
        }

        // Validate source ≠ destination
        if (fromAccountId == toAccountId)
        {
            return TransferResult.Failure(
                "InvalidTransfer",
                "Cannot transfer to the same account.");
        }

        // Retrieve both accounts with AsNoTracking to avoid concurrency issues
        var fromAccount = await this.context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == fromAccountId);

        if (fromAccount == null)
        {
            return TransferResult.Failure(
                "AccountNotFound",
                $"Source account with ID {fromAccountId} not found.");
        }

        var toAccount = await this.context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == toAccountId);

        if (toAccount == null)
        {
            return TransferResult.Failure(
                "AccountNotFound",
                $"Destination account with ID {toAccountId} not found.");
        }

        // Verify user owns both accounts
        if (fromAccount.UserId != userId || toAccount.UserId != userId)
        {
            return TransferResult.Failure(
                "Unauthorized",
                "User does not own one or both accounts.");
        }

        // Check for sufficient balance
        if (fromAccount.Balance < amount)
        {
            return TransferResult.Failure(
                "InsufficientBalance",
                "Transfer amount exceeds available balance.",
                new { available = fromAccount.Balance, requested = amount });
        }

        // Validate both accounts are active
        if (!fromAccount.IsActive || !toAccount.IsActive)
        {
            return TransferResult.Failure(
                "InvalidTransfer",
                "One or both accounts are inactive.");
        }

        // Validate currencies match
        if (fromAccount.Currency != toAccount.Currency)
        {
            return TransferResult.Failure(
                "InvalidTransfer",
                "Accounts must have the same currency for transfers.");
        }

        try
        {
            // Re-fetch accounts in tracking mode for modification
            var trackingFromAccount = await this.context.Accounts
                .FirstAsync(a => a.Id == fromAccountId);

            var trackingToAccount = await this.context.Accounts
                .FirstAsync(a => a.Id == toAccountId);

            // Create debit transaction in source account
            var now = DateTime.UtcNow;
            var sourceDescription = $"Transfer to {trackingToAccount.AccountNumber}";
            var sourceTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = fromAccountId,
                Amount = amount,
                Date = now,
                Description = sourceDescription,
                Category = DomainConstants.TransactionCategories.Transfer,
                Type = DomainConstants.TransactionTypes.Debit,
            };

            // Create credit transaction in destination account
            var destinationDescription = $"Transfer from {trackingFromAccount.AccountNumber}";
            var destinationTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = toAccountId,
                Amount = amount,
                Date = now,
                Description = destinationDescription,
                Category = DomainConstants.TransactionCategories.Transfer,
                Type = DomainConstants.TransactionTypes.Credit,
            };

            // Update balances
            trackingFromAccount.Balance -= amount;
            trackingFromAccount.LastUpdated = now;
            trackingToAccount.Balance += amount;
            trackingToAccount.LastUpdated = now;

            // Add transactions
            this.context.Transactions.Add(sourceTransaction);
            this.context.Transactions.Add(destinationTransaction);

            // Save all changes atomically
            await this.context.SaveChangesAsync();

            return TransferResult.Success(
                fromAccountId,
                toAccountId,
                amount,
                trackingFromAccount.Balance,
                trackingToAccount.Balance);
        }
        catch (InvalidOperationException ex)
        {
            return TransferResult.Failure(
                "InvalidTransfer",
                ex.Message);
        }
    }
}
