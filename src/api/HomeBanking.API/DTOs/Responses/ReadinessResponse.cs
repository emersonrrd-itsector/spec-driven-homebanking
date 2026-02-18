namespace HomeBanking.API.DTOs.Responses;

/// <summary>
/// Response DTO for the GET /api/health/ready endpoint.
/// Returns the readiness status, timestamp, and details about database connectivity.
/// </summary>
public class ReadinessResponse
{
    /// <summary>
    /// Gets or sets the readiness status of the API.
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the readiness check was performed (ISO8601 format, UTC).
    /// </summary>
    public required DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets optional details about the readiness check (e.g., database connection status).
    /// </summary>
    public object? Details { get; set; }
}
