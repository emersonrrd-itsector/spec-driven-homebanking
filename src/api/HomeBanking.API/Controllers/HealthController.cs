namespace HomeBanking.API.Controllers;

using HomeBanking.API.Data;
using HomeBanking.API.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// API controller for health check endpoints.
/// Provides endpoints to check API health and database readiness.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private const string ApiVersion = "1.0.0";
    private readonly HomeBankingContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthController"/> class.
    /// </summary>
    /// <param name="dbContext">The database context for readiness checks.</param>
    /// <exception cref="ArgumentNullException">Thrown when dbContext is null.</exception>
    public HealthController(HomeBankingContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Returns the health status of the API.
    /// This endpoint is always available and does not require authentication.
    /// </summary>
    /// <returns>
    /// 200 OK with HealthResponse containing status, timestamp (ISO8601), and API version.
    /// </returns>
    [HttpGet("/health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        var response = new HealthResponse
        {
            Status = "healthy",
            Timestamp = DateTime.UtcNow,
            Version = ApiVersion,
        };

        return this.Ok(response);
    }

    /// <summary>
    /// Checks the readiness of the API by verifying database connectivity.
    /// This endpoint is always available and does not require authentication.
    /// </summary>
    /// <returns>
    /// 200 OK with ReadinessResponse if the database is accessible.
    /// 503 Service Unavailable with error details if the database is not accessible.
    /// </returns>
    [HttpGet("ready")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReadinessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReadinessResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Ready()
    {
        try
        {
            // Check database connectivity by executing a simple query
            var canConnect = await this.dbContext.Database.CanConnectAsync();

            if (!canConnect)
            {
                return this.StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new ReadinessResponse
                    {
                        Status = "not_ready",
                        Timestamp = DateTime.UtcNow,
                        Details = new { error = "Database connection failed" },
                    });
            }

            // Attempt to execute a simple query to verify database is truly accessible
            _ = await this.dbContext.Users.FirstOrDefaultAsync();

            return this.Ok(new ReadinessResponse
            {
                Status = "ready",
                Timestamp = DateTime.UtcNow,
                Details = new { database = "connected" },
            });
        }
        catch (Exception ex)
        {
            return this.StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ReadinessResponse
                {
                    Status = "not_ready",
                    Timestamp = DateTime.UtcNow,
                    Details = new { error = ex.Message },
                });
        }
    }
}
