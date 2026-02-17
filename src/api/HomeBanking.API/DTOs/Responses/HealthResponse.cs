namespace HomeBanking.API.DTOs.Responses;

/// <summary>
/// Response DTO for the GET /api/health endpoint.
/// Returns the health status, timestamp, and version of the API.
/// </summary>
public class HealthResponse
{
    /// <summary>
    /// Gets or sets the health status of the API.
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the health check was performed (ISO8601 format, UTC).
    /// </summary>
    public required DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the API version.
    /// </summary>
    public required string Version { get; set; }
}
