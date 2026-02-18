namespace HomeBanking.Tests.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HomeBanking.API.Services;
using HomeBanking.Domain;

/// <summary>
/// Unit tests for JwtTokenService.
/// </summary>
public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        _service = new JwtTokenService("test-secret-key-long-enough-for-256bits", 24);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsJwtToken()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash",
            IsActive = true,
        };

        // Act
        var token = _service.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.IsType<string>(token);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ContainsUserIdClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hash",
            IsActive = true,
        };

        // Act
        var token = _service.GenerateToken(user);
        var principal = DecodeToken(token);

        // Assert
        Assert.NotNull(principal);
        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
        Assert.NotNull(userIdClaim);
        Assert.Equal(userId.ToString(), userIdClaim.Value);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ContainsEmailClaim()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hash",
            IsActive = true,
        };

        // Act
        var token = _service.GenerateToken(user);
        var principal = DecodeToken(token);

        // Assert
        Assert.NotNull(principal);
        var emailClaim = principal.FindFirst(JwtRegisteredClaimNames.Email);
        Assert.NotNull(emailClaim);
        Assert.Equal(email, emailClaim.Value);
    }

    [Fact]
    public void GenerateToken_WithValidUser_TokenContainsJtiClaim()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash",
            IsActive = true,
        };

        // Act
        var token = _service.GenerateToken(user);
        var principal = DecodeToken(token);

        // Assert
        Assert.NotNull(principal);
        var jtiClaim = principal.FindFirst(JwtRegisteredClaimNames.Jti);
        Assert.NotNull(jtiClaim);
        Assert.NotEmpty(jtiClaim.Value);
    }

    [Fact]
    public void GenerateToken_WithNullUser_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.GenerateToken(null!));
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash",
            IsActive = true,
        };
        var token = _service.GenerateToken(user);

        // Act
        var principal = _service.ValidateToken(token);

        // Assert
        Assert.NotNull(principal);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ContainsCorrectClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var user = new User
        {
            Id = userId,
            Email = email,
            PasswordHash = "hash",
            IsActive = true,
        };
        var token = _service.GenerateToken(user);

        // Act
        var principal = _service.ValidateToken(token);

        // Assert
        Assert.NotNull(principal);
        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
        var emailClaim = principal.FindFirst(JwtRegisteredClaimNames.Email);
        Assert.NotNull(userIdClaim);
        Assert.NotNull(emailClaim);
        Assert.Equal(userId.ToString(), userIdClaim.Value);
        Assert.Equal(email, emailClaim.Value);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
        // Act
        var principal = _service.ValidateToken("invalid-token");

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void ValidateToken_WithEmptyToken_ReturnsNull()
    {
        // Act
        var principal = _service.ValidateToken(string.Empty);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void ValidateToken_WithNullToken_ReturnsNull()
    {
        // Act
        var principal = _service.ValidateToken(null!);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void ValidateToken_WithWrongSecret_ReturnsNull()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash",
            IsActive = true,
        };
        var token = _service.GenerateToken(user);

        // Create new service with different secret
        var wrongSecretService = new JwtTokenService("wrong-secret-key-long-enough-for-256bits", 24);

        // Act
        var principal = wrongSecretService.ValidateToken(token);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void GenerateToken_TokenHas24HourExpiry()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash",
            IsActive = true,
        };
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _service.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
        var afterGeneration = DateTime.UtcNow;

        // Assert
        Assert.NotNull(jwtToken);
        var expectedExpiry = beforeGeneration.AddHours(24);
        var actualExpiry = jwtToken.ValidTo;

        // Allow 5 seconds of tolerance for execution time
        var tolerance = TimeSpan.FromSeconds(5);
        Assert.InRange(actualExpiry, expectedExpiry - tolerance, expectedExpiry + tolerance);
    }

    /// <summary>
    /// Helper method to decode and validate token for testing.
    /// </summary>
    private ClaimsPrincipal? DecodeToken(string token)
    {
        return _service.ValidateToken(token);
    }
}
