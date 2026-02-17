namespace HomeBanking.Domain;

/// <summary>
/// Represents a money value object with amount and currency.
/// Immutable value object ensuring amount is non-negative and currency is valid.
/// </summary>
public class Money
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class.
    /// </summary>
    /// <param name="amount">The monetary amount (must be >= 0).</param>
    /// <param name="currency">The currency code (must be exactly 3 uppercase characters).</param>
    /// <exception cref="ArgumentException">Thrown when amount is negative or currency format is invalid.</exception>
    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        {
            throw new ArgumentException("Currency must be exactly 3 characters.", nameof(currency));
        }

        this.Amount = amount;
        this.Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Gets the monetary amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the currency code (3-character ISO code).
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Creates a new Money value object with validation.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The currency code.</param>
    /// <returns>A new Money instance.</returns>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    public static Money Create(decimal amount, string currency)
    {
        return new Money(amount, currency);
    }

    /// <summary>
    /// Returns a string representation of the money value in format "CURRENCY AMOUNT".
    /// </summary>
    /// <returns>A formatted string (e.g., "USD 1000.00").</returns>
    public override string ToString()
    {
        return $"{this.Currency} {this.Amount:N2}";
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current Money object.
    /// Two Money objects are equal if both amount and currency match.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>True if the objects are equal; otherwise false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not Money other)
        {
            return false;
        }

        return this.Amount == other.Amount && this.Currency == other.Currency;
    }

    /// <summary>
    /// Serves as the default hash function for Money.
    /// </summary>
    /// <returns>A hash code for the current Money object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Amount, this.Currency);
    }
}
