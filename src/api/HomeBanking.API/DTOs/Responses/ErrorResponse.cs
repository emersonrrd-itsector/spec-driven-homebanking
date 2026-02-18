namespace HomeBanking.API.DTOs.Responses;

/// <summary>
/// Response DTO for API errors with code, message, and optional details.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Gets or sets the error code (e.g., "InvalidCredentials", "NotFound").
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// Gets or sets the human-readable error message.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Gets or sets optional error details (additional context).
    /// </summary>
    public object? Details { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the error occurred (UTC).
    /// </summary>
    public required DateTime Timestamp { get; set; }
}
