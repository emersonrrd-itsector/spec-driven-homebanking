namespace HomeBanking.API.Controllers;

using HomeBanking.API.DTOs.Requests;
using HomeBanking.API.DTOs.Responses;
using HomeBanking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API controller for user authentication endpoints.
/// Handles user login and JWT token generation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    /// <exception cref="ArgumentNullException">Thrown when authService is null.</exception>
    public AuthController(IAuthService authService)
    {
        this.authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    /// <summary>
    /// Authenticates a user with email and password, returning a JWT token.
    /// </summary>
    /// <param name="request">The login request containing email and password.</param>
    /// <returns>
    /// 200 OK with LoginResponse (token, expiresIn, user) on successful authentication.
    /// 401 Unauthorized with ErrorResponse if credentials are invalid.
    /// </returns>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
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

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return this.BadRequest(new ErrorResponse
            {
                Code = "InvalidRequest",
                Message = "Email and password are required.",
                Details = null,
                Timestamp = DateTime.UtcNow,
            });
        }

        try
        {
            var response = await this.authService.LoginAsync(request.Email, request.Password);
            return this.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.Unauthorized(new ErrorResponse
            {
                Code = "InvalidCredentials",
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
                Message = "An unexpected error occurred during login.",
                Details = ex.Message,
                Timestamp = DateTime.UtcNow,
            });
        }
    }
}
