namespace HomeBanking.API.DTOs.Responses;

/// <summary>
/// Response envelope for paginated transaction list.
/// </summary>
public class TransactionsResponse
{
    /// <summary>
    /// Gets or sets the list of transactions for the current page.
    /// </summary>
    public required IEnumerable<TransactionDto> Transactions { get; set; }

    /// <summary>
    /// Gets or sets the total count of transactions (across all pages) matching the filter criteria.
    /// </summary>
    public required int Total { get; set; }

    /// <summary>
    /// Gets or sets the number of items skipped from the beginning (pagination offset).
    /// </summary>
    public required int Skip { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items returned in this response (page size).
    /// </summary>
    public required int Take { get; set; }
}
