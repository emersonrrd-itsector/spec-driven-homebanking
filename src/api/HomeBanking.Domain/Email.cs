namespace HomeBanking.Domain;

using System.Text.RegularExpressions;

/// <summary>
/// Represents an email address value object.
/// Immutable value object ensuring email is in valid format.
/// </summary>
public class Email
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> class.
    /// </summary>
    /// <param name="value">The email address (must be valid format).</param>
    /// <exception cref="ArgumentException">Thrown when email format is invalid.</exception>
    public Email(string value)
    {
        if (!IsValidFormat(value))
        {
            throw new ArgumentException("Email format is invalid.", nameof(value));
        }

        this.Value = value;
    }

    /// <summary>
    /// Gets the email address value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new Email value object with validation.
    /// </summary>
    /// <param name="value">The email address.</param>
    /// <returns>A new Email instance.</returns>
    /// <exception cref="ArgumentException">Thrown if email format is invalid.</exception>
    public static Email Create(string value)
    {
        return new Email(value);
    }

    /// <summary>
    /// Validates whether a given string is in valid email format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the email is valid; otherwise false.</returns>
    public static bool IsValidFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the email address as a string.
    /// </summary>
    /// <returns>The email address value.</returns>
    public override string ToString()
    {
        return this.Value;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current Email object.
    /// Two Email objects are equal if their values match (case-insensitive).
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>True if the objects are equal; otherwise false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not Email other)
        {
            return false;
        }

        return string.Equals(this.Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Serves as the default hash function for Email.
    /// </summary>
    /// <returns>A hash code for the current Email object.</returns>
    public override int GetHashCode()
    {
        return this.Value.ToLowerInvariant().GetHashCode();
    }
}
