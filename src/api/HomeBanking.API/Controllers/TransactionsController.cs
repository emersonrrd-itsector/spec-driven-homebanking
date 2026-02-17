namespace HomeBanking.API.Controllers;

using System.Security.Claims;
using HomeBanking.API.DTOs.Responses;
using HomeBanking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API controller for transaction endpoints.
/// Provides endpoints to retrieve and filter transactions for user accounts.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService transactionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionsController"/> class.
    /// </summary>
    /// <param name="transactionService">The transaction service.</param>
    /// <exception cref="ArgumentNullException">Thrown when transactionService is null.</exception>
    public TransactionsController(ITransactionService transactionService)
    {
        this.transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
    }

    /// <summary>
    /// Retrieves paginated transactions for a specific account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account (required query parameter).</param>
    /// <param name="skip">Number of transactions to skip for pagination (default: 0).</param>
    /// <param name="take">Maximum number of transactions to return (default: 10, max: 100).</param>
    /// <param name="category">Optional category filter to return only matching transactions.</param>
    /// <returns>
    /// 200 OK with TransactionsResponse containing paginated transactions.
    /// 400 Bad Request if query parameters are invalid.
    /// 401 Unauthorized if JWT token is missing or invalid.
    /// 500 Internal Server Error on server-side failures.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(TransactionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] Guid accountId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        [FromQuery] string? category = null)
    {
        try
        {
            var userIdClaim = this.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return this.Unauthorized(new ErrorResponse
                {
                    Code = "Unauthorized",
                    Message = "User ID not found in token.",
                    Details = null,
                    Timestamp = DateTime.UtcNow,
                });
            }

            if (accountId == Guid.Empty)
            {
                return this.BadRequest(new ErrorResponse
                {
                    Code = "InvalidRequest",
                    Message = "Account ID cannot be empty.",
                    Details = null,
                    Timestamp = DateTime.UtcNow,
                });
            }

            var (transactions, total) = await this.transactionService.GetAccountTransactionsAsync(
                userId,
                accountId,
                skip,
                take,
                category);

            return this.Ok(new TransactionsResponse
            {
                Transactions = transactions,
                Total = total,
                Skip = skip,
                Take = take,
            });
        }
        catch (ArgumentException ex)
        {
            return this.BadRequest(new ErrorResponse
            {
                Code = "InvalidRequest",
                Message = ex.Message,
                Details = null,
                Timestamp = DateTime.UtcNow,
            });
        }
        catch (Exception ex)
        {
            return this.StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Code = "InternalServerError",
                Message = "An error occurred while retrieving transactions.",
                Details = ex.Message,
                Timestamp = DateTime.UtcNow,
            });
        }
    }
}
