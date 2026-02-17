namespace HomeBanking.API.Services;

using HomeBanking.API.DTOs.Responses;

/// <summary>
/// Interface for authentication service operations.
/// Handles user login and token generation.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with email and password, returning a JWT token on success.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>A LoginResponse containing the JWT token and user information.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid.</exception>
    Task<LoginResponse> LoginAsync(string email, string password);
}
