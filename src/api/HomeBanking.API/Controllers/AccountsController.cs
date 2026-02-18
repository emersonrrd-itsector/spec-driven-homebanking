namespace HomeBanking.API.Controllers;

using System.Security.Claims;
using HomeBanking.API.DTOs.Responses;
using HomeBanking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API controller for account endpoints.
/// Provides endpoints to retrieve user accounts and account details.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService accountService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountsController"/> class.
    /// </summary>
    /// <param name="accountService">The account service.</param>
    /// <exception cref="ArgumentNullException">Thrown when accountService is null.</exception>
    public AccountsController(IAccountService accountService)
    {
        this.accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
    }

    /// <summary>
    /// Retrieves all accounts for the authenticated user.
    /// </summary>
    /// <returns>
    /// 200 OK with AccountsResponse containing array of accounts.
    /// 401 Unauthorized if JWT token is missing or invalid.
    /// 500 Internal Server Error on server-side failures.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(AccountsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAccounts()
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

            var accounts = await this.accountService.GetUserAccountsAsync(userId);
            return this.Ok(new AccountsResponse { Accounts = accounts });
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
                Message = "An error occurred while retrieving accounts.",
                Details = ex.Message,
                Timestamp = DateTime.UtcNow,
            });
        }
    }

    /// <summary>
    /// Retrieves a specific account by ID for the authenticated user.
    /// </summary>
    /// <param name="id">The unique identifier of the account.</param>
    /// <returns>
    /// 200 OK with the AccountDto.
    /// 401 Unauthorized if JWT token is missing or invalid.
    /// 404 Not Found if account does not exist or does not belong to the user.
    /// 500 Internal Server Error on server-side failures.
    /// </returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAccount(Guid id)
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

            if (id == Guid.Empty)
            {
                return this.BadRequest(new ErrorResponse
                {
                    Code = "InvalidRequest",
                    Message = "Account ID cannot be empty.",
                    Details = null,
                    Timestamp = DateTime.UtcNow,
                });
            }

            var account = await this.accountService.GetUserAccountAsync(userId, id);
            if (account == null)
            {
                return this.NotFound(new ErrorResponse
                {
                    Code = "AccountNotFound",
                    Message = "Account not found or does not belong to the user.",
                    Details = null,
                    Timestamp = DateTime.UtcNow,
                });
            }

            return this.Ok(account);
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
                Message = "An error occurred while retrieving the account.",
                Details = ex.Message,
                Timestamp = DateTime.UtcNow,
            });
        }
    }
}
