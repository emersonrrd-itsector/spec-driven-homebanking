namespace HomeBanking.API.Services;

using HomeBanking.API.Data;
using HomeBanking.API.DTOs.Responses;

/// <summary>
/// Service for handling user authentication and JWT token generation.
/// </summary>
public class AuthService : IAuthService
{
    private readonly HomeBankingContext context;
    private readonly JwtTokenService jwtTokenService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="context">The HomeBanking database context.</param>
    /// <param name="jwtTokenService">The JWT token service for generating tokens.</param>
    /// <exception cref="ArgumentNullException">Thrown when context or jwtTokenService is null.</exception>
    public AuthService(HomeBankingContext context, JwtTokenService jwtTokenService)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
    }

    /// <summary>
    /// Authenticates a user with email and password, returning a JWT token on success.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>A LoginResponse containing the JWT token and user information.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid.</exception>
    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        return await Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new UnauthorizedAccessException("Email and password are required.");
            }

            // Find user by email
            var user = this.context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Email or password incorrect.");
            }

            // Verify password hash
            if (!user.PasswordHashMatches(password))
            {
                throw new UnauthorizedAccessException("Email or password incorrect.");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("User account is not active.");
            }

            // Generate JWT token
            var token = this.jwtTokenService.GenerateToken(user);
            const int tokenExpiryHours = 24;
            const int tokenExpirySeconds = tokenExpiryHours * 3600;

            return new LoginResponse
            {
                Token = token,
                ExpiresIn = tokenExpirySeconds,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                },
            };
        });
    }
}
