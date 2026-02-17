namespace HomeBanking.API.DTOs.Responses;

/// <summary>
/// Response DTO for user information.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Gets or sets the user's unique identifier.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public required string Email { get; set; }
}
