namespace HomeBanking.API.DTOs.Responses;

/// <summary>
/// Response DTO wrapping a collection of accounts for the GET /api/accounts endpoint.
/// </summary>
public class AccountsResponse
{
    /// <summary>
    /// Gets or sets the list of accounts belonging to the user.
    /// </summary>
    public required IEnumerable<AccountDto> Accounts { get; set; }
}
