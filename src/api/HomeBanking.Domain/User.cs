namespace HomeBanking.Domain;

/// <summary>
/// Represents a user of the HomeBanking application.
/// Aggregate root: encapsulates user identity, authentication, and account management.
/// </summary>
public class User
{
    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    public User()
    {
        // Required by EF Core for parameterless constructor
    }

    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the email address of the user (unique).
    /// Must be in valid email format.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the SHA-256 hash of the user's password (for demo purposes; production should use bcrypt).
    /// Cannot be null or empty.
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// Default: true.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets the collection of accounts owned by this user.
    /// </summary>
    public ICollection<Account> Accounts { get; init; } = new List<Account>();

    /// <summary>
    /// Validates that the user's email address is in a valid format.
    /// </summary>
    /// <returns>True if the email format is valid; otherwise false.</returns>
    public bool ValidateEmailFormat()
    {
        return HomeBanking.Domain.Email.IsValidFormat(this.Email);
    }

    /// <summary>
    /// Determines whether a given plain text password matches the user's stored password hash.
    /// </summary>
    /// <param name="plainPassword">The plain text password to verify.</param>
    /// <returns>True if the password matches the hash; otherwise false.</returns>
    public bool PasswordHashMatches(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword) || string.IsNullOrWhiteSpace(this.PasswordHash))
        {
            return false;
        }

        try
        {
            var passwordHash = new PasswordHash(this.PasswordHash);
            return passwordHash.Matches(plainPassword);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Retrieves an account by its ID from this user's account collection.
    /// </summary>
    /// <param name="accountId">The ID of the account to retrieve.</param>
    /// <returns>The account if found; otherwise null.</returns>
    public Account? GetAccountById(Guid accountId)
    {
        return this.Accounts.FirstOrDefault(a => a.Id == accountId);
    }

    /// <summary>
    /// Gets all active accounts for this user.
    /// </summary>
    /// <returns>A collection of active accounts.</returns>
    public IEnumerable<Account> GetActiveAccounts()
    {
        return this.Accounts.Where(a => a.IsActive);
    }

    /// <summary>
    /// Determines whether this user owns a specific account.
    /// </summary>
    /// <param name="account">The account to check.</param>
    /// <returns>True if the user owns the account; otherwise false.</returns>
    public bool OwnsAccount(Account account)
    {
        return account.UserId == this.Id;
    }
}
