namespace HomeBanking.API.Controllers;

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using HomeBanking.API.DTOs.Requests;
using HomeBanking.API.DTOs.Responses;
using HomeBanking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API controller for transfer endpoints.
/// Provides endpoints to execute transfers between user accounts.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransfersController : ControllerBase
{
    private readonly ITransferService transferService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransfersController"/> class.
    /// </summary>
    /// <param name="transferService">The transfer service.</param>
    /// <exception cref="ArgumentNullException">Thrown when transferService is null.</exception>
    public TransfersController(ITransferService transferService)
    {
        this.transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
    }

    /// <summary>
    /// Executes a transfer from one account to another.
    /// </summary>
    /// <param name="request">The transfer request containing source account, destination account, amount, and optional description.</param>
    /// <returns>
    /// 201 Created with transfer details (transferId, status, fromAccount, toAccount, amount) on successful transfer.
    /// 400 Bad Request if input validation fails or transfer business logic fails (insufficient balance, account not found, etc.).
    /// 401 Unauthorized if JWT token is missing or invalid.
    /// 500 Internal Server Error on server-side failures.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostTransfer([FromBody] TransferRequest request)
    {
        // Input null check
        if (request == null)
        {
            return this.BadRequest(new ErrorResponse
            {
                Code = "InvalidRequest",
                Message = "Request body is required.",
                Details = null,
                Timestamp = DateTime.UtcNow,
            });
        }

        // Input model validation
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, context, results, true))
        {
            var validationErrors = string.Join("; ", results.Select(r => r.ErrorMessage));
            return this.BadRequest(new ErrorResponse
            {
                Code = "ValidationError",
                Message = "Transfer request validation failed.",
                Details = validationErrors,
                Timestamp = DateTime.UtcNow,
            });
        }

        try
        {
            // Extract userId from JWT claims
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

            // Call transfer service
            var transferResult = await this.transferService.ExecuteTransferAsync(
                request.FromAccountId,
                request.ToAccountId,
                request.Amount,
                request.Description,
                userId);

            // Handle transfer failure
            if (!transferResult.IsSuccess)
            {
                return this.BadRequest(new ErrorResponse
                {
                    Code = transferResult.ErrorCode ?? "TransferFailed",
                    Message = transferResult.ErrorMessage ?? "Transfer failed.",
                    Details = transferResult.ErrorDetails,
                    Timestamp = DateTime.UtcNow,
                });
            }

            // Return success response with 201 Created status
            var response = new TransferResponse
            {
                TransferId = Guid.NewGuid().ToString(), // In a real system, this would come from the service
                Status = "completed",
                FromAccount = new TransferAccountResponse
                {
                    Id = transferResult.FromAccountId!.Value,
                    NewBalance = transferResult.FromAccountNewBalance!.Value,
                },
                ToAccount = new TransferAccountResponse
                {
                    Id = transferResult.ToAccountId!.Value,
                    NewBalance = transferResult.ToAccountNewBalance!.Value,
                },
                Amount = transferResult.Amount!.Value,
            };

            return this.StatusCode(StatusCodes.Status201Created, response);
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
                Message = "An error occurred while processing the transfer.",
                Details = ex.Message,
                Timestamp = DateTime.UtcNow,
            });
        }
    }
}
