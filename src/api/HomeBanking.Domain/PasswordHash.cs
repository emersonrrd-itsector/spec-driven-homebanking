namespace HomeBanking.Domain;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Represents a password hash value object.
/// Encapsulates SHA-256 password hashing logic for verification.
/// </summary>
public class PasswordHash
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordHash"/> class.
    /// </summary>
    /// <param name="hash">The stored SHA-256 hash string.</param>
    /// <exception cref="ArgumentException">Thrown when hash is null or empty.</exception>
    public PasswordHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ArgumentException("Hash cannot be null or empty.", nameof(hash));
        }

        this.Hash = hash;
    }

    /// <summary>
    /// Gets the stored hash value.
    /// </summary>
    public string Hash { get; }

    /// <summary>
    /// Creates a new password hash from plain text.
    /// </summary>
    /// <param name="plainPassword">The plain text password to hash.</param>
    /// <returns>A new PasswordHash instance.</returns>
    public static PasswordHash Create(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(plainPassword));
        }

        var hash = ComputeSha256HashStatic(plainPassword);
        return new PasswordHash(hash);
    }

    /// <summary>
    /// Creates a new password hash from plain text password.
    /// </summary>
    /// <param name="plainPassword">The plain text password to hash.</param>
    /// <returns>A new PasswordHash instance with the computed hash.</returns>
    public static PasswordHash CreateFromPlainText(string plainPassword)
    {
        return Create(plainPassword);
    }

    /// <summary>
    /// Verifies whether a plain text password matches this hash.
    /// </summary>
    /// <param name="plainPassword">The plain text password to verify.</param>
    /// <returns>True if the password matches; otherwise false.</returns>
    public bool Matches(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
        {
            return false;
        }

        try
        {
            var computedHash = this.ComputeSha256Hash(plainPassword);
            return string.Equals(computedHash, this.Hash, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the hash string representation.
    /// </summary>
    /// <returns>The hash value.</returns>
    public override string ToString()
    {
        return this.Hash;
    }

    /// <summary>
    /// Static method for computing SHA-256 hash of a plain text password.
    /// </summary>
    /// <param name="input">The plain text password.</param>
    /// <returns>The SHA-256 hash as a hexadecimal string.</returns>
    private static string ComputeSha256HashStatic(string input)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hashedBytes);
        }
    }

    /// <summary>
    /// Computes SHA-256 hash of a plain text password.
    /// </summary>
    /// <param name="input">The plain text password.</param>
    /// <returns>The SHA-256 hash as a hexadecimal string.</returns>
    private string ComputeSha256Hash(string input)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hashedBytes);
        }
    }
}
