namespace HomeBanking.API.DTOs.Responses;

/// <summary>
/// Response DTO for login with JWT token and user information.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets the JWT authentication token.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// Gets or sets the token expiry duration in seconds.
    /// </summary>
    public required int ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the user information.
    /// </summary>
    public required UserDto User { get; set; }
}
