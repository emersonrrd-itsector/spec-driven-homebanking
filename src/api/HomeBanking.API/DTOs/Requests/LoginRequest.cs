namespace HomeBanking.API.DTOs.Requests;

/// <summary>
/// Request DTO for user login with email and password.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the user's password.
    /// </summary>
    public required string Password { get; set; }
}
