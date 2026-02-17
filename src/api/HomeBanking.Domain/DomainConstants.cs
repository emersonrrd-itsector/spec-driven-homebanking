namespace HomeBanking.Domain;

/// <summary>
/// Contains domain-wide constants for validation, business rules, and predefined values.
/// </summary>
public static class DomainConstants
{
    /// <summary>
    /// Predefined transaction categories.
    /// </summary>
    public static class TransactionCategories
    {
        /// <summary>
        /// Groceries category.
        /// </summary>
        public const string Groceries = "Groceries";

        /// <summary>
        /// Utilities category.
        /// </summary>
        public const string Utilities = "Utilities";

        /// <summary>
        /// Entertainment category.
        /// </summary>
        public const string Entertainment = "Entertainment";

        /// <summary>
        /// Food &amp; Dining category.
        /// </summary>
        public const string FoodDining = "Food & Dining";

        /// <summary>
        /// Transport category.
        /// </summary>
        public const string Transport = "Transport";

        /// <summary>
        /// Salary category.
        /// </summary>
        public const string Salary = "Salary";

        /// <summary>
        /// Transfer category.
        /// </summary>
        public const string Transfer = "Transfer";

        /// <summary>
        /// Other category.
        /// </summary>
        public const string Other = "Other";

        /// <summary>
        /// Gets all predefined categories.
        /// </summary>
        public static IReadOnlyList<string> AllCategories => new[]
        {
            Groceries, Utilities, Entertainment, FoodDining, Transport, Salary, Transfer, Other,
        };
    }

    /// <summary>
    /// Predefined currency codes.
    /// </summary>
    public static class CurrencyCodes
    {
        /// <summary>
        /// US Dollar.
        /// </summary>
        public const string USD = "USD";

        /// <summary>
        /// Euro.
        /// </summary>
        public const string EUR = "EUR";

        /// <summary>
        /// British Pound.
        /// </summary>
        public const string GBP = "GBP";

        /// <summary>
        /// Canadian Dollar.
        /// </summary>
        public const string CAD = "CAD";

        /// <summary>
        /// Australian Dollar.
        /// </summary>
        public const string AUD = "AUD";

        /// <summary>
        /// Japanese Yen.
        /// </summary>
        public const string JPY = "JPY";

        /// <summary>
        /// Brazilian Real.
        /// </summary>
        public const string BRL = "BRL";

        /// <summary>
        /// Gets all supported currency codes.
        /// </summary>
        public static IReadOnlyList<string> AllCurrencies => new[]
        {
            USD, EUR, GBP, CAD, AUD, JPY, BRL,
        };
    }

    /// <summary>
    /// Transaction type constants.
    /// </summary>
    public static class TransactionTypes
    {
        /// <summary>
        /// Debit transaction type.
        /// </summary>
        public const string Debit = "Debit";

        /// <summary>
        /// Credit transaction type.
        /// </summary>
        public const string Credit = "Credit";

        /// <summary>
        /// Gets all transaction types.
        /// </summary>
        public static IReadOnlyList<string> AllTypes => new[] { Debit, Credit };
    }

    /// <summary>
    /// Validation constraints for transactions and transfers.
    /// </summary>
    public static class TransactionLimits
    {
        /// <summary>
        /// Minimum allowed transaction amount (in cents, e.g., $0.01).
        /// </summary>
        public const decimal MinTransactionAmount = 0.01m;

        /// <summary>
        /// Maximum allowed single transaction amount (100,000 in base currency).
        /// </summary>
        public const decimal MaxTransactionAmount = 100_000m;

        /// <summary>
        /// Minimum balance allowed (0.00).
        /// </summary>
        public const decimal MinBalance = 0m;
    }
}
