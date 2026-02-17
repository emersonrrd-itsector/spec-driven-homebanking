namespace HomeBanking.API.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HomeBanking.Domain;
using Microsoft.IdentityModel.Tokens;

#pragma warning disable CS8604 // Possible null reference argument.

/// <summary>
/// Service for generating and validating JWT tokens.
/// Implements token generation with user claims and expiry validation.
/// </summary>
public class JwtTokenService
{
    private readonly string secret;
    private readonly int expiryHours;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// </summary>
    /// <param name="secret">The JWT secret key from configuration. If null or empty, uses default demo key.</param>
    /// <param name="expiryHours">The token expiry duration in hours. Defaults to 24.</param>
    public JwtTokenService(string? secret = null, int expiryHours = 24)
    {
        this.secret = secret ?? "demo-secret-key-long-enough-for-256bits";
        this.expiryHours = expiryHours;
    }

    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate a token for.</param>
    /// <returns>A JWT token string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    public string GenerateToken(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "User cannot be null.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(this.expiryHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates a JWT token and returns the claims principal if valid.
    /// </summary>
    /// <param name="token">The JWT token string to validate.</param>
    /// <returns>A ClaimsPrincipal if the token is valid; null if invalid or expired.</returns>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.secret));
            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                },
                out SecurityToken validatedToken);

            // Ensure all claims from the token are included in the principal
            if (validatedToken is JwtSecurityToken jwtToken && principal != null)
            {
                var claims = new List<Claim>(principal.Claims);
                foreach (var claim in jwtToken.Claims.Where(c => !claims.Any(pc => pc.Type == c.Type && pc.Value == c.Value)))
                {
                    claims.Add(claim);
                }

                return new ClaimsPrincipal(new ClaimsIdentity(claims, "JWT"));
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}

#pragma warning restore CS8604 // Possible null reference argument.
