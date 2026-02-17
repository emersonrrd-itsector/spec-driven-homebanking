namespace HomeBanking.Domain;

/// <summary>
/// Represents a user of the HomeBanking application.
/// </summary>
public class User
{
    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the email address of the user (unique).
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the SHA-256 hash of the user's password (for demo purposes; production should use bcrypt).
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Gets the collection of accounts owned by this user.
    /// </summary>
    public ICollection<Account> Accounts { get; init; } = new List<Account>();
}
