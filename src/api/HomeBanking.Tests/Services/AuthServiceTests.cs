namespace HomeBanking.Tests.Services;

using HomeBanking.API.Data;
using HomeBanking.API.Services;
using HomeBanking.Domain;
using Microsoft.EntityFrameworkCore;
using Moq;

/// <summary>
/// Unit tests for AuthService.
/// </summary>
public class AuthServiceTests
{
    private readonly HomeBankingContext _context;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<HomeBankingContext>()
            .UseInMemoryDatabase(databaseName: $"test_db_{Guid.NewGuid()}")
            .Options;

        _context = new HomeBankingContext(options);

        // Seed test data
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "admin@homebank.local",
            PasswordHash = PasswordHash.CreateFromPlainText("demo123").Hash,
            IsActive = true,
        };
        _context.Users.Add(user);
        _context.SaveChanges();

        var jwtService = new JwtTokenService("test-secret-key-long-enough-for-256bits", 24);
        _service = new AuthService(_context, jwtService);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse()
    {
        // Act
        var response = await _service.LoginAsync("admin@homebank.local", "demo123");

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Token);
        Assert.Equal(86400, response.ExpiresIn); // 24 hours in seconds
        Assert.NotNull(response.User);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsCorrectUserInfo()
    {
        // Act
        var response = await _service.LoginAsync("admin@homebank.local", "demo123");

        // Assert
        Assert.NotNull(response.User);
        Assert.Equal("admin@homebank.local", response.User.Email);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ThrowsUnauthorizedAccessException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync("nonexistent@example.com", "demo123"));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync("admin@homebank.local", "wrongpassword"));
    }

    [Fact]
    public async Task LoginAsync_WithEmptyEmail_ThrowsUnauthorizedAccessException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(string.Empty, "demo123"));
    }

    [Fact]
    public async Task LoginAsync_WithEmptyPassword_ThrowsUnauthorizedAccessException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync("admin@homebank.local", string.Empty));
    }

    [Fact]
    public async Task LoginAsync_WithNullEmail_ThrowsUnauthorizedAccessException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(null!, "demo123"));
    }

    [Fact]
    public async Task LoginAsync_WithNullPassword_ThrowsUnauthorizedAccessException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync("admin@homebank.local", null!));
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var inactiveUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@example.com",
            PasswordHash = PasswordHash.CreateFromPlainText("password123").Hash,
            IsActive = false,
        };
        _context.Users.Add(inactiveUser);
        _context.SaveChanges();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync("inactive@example.com", "password123"));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsNonEmptyToken()
    {
        // Act
        var response = await _service.LoginAsync("admin@homebank.local", "demo123");

        // Assert
        Assert.NotNull(response.Token);
        Assert.NotEmpty(response.Token);
        Assert.True(response.Token.Length > 50); // JWT tokens are typically 100+ characters
    }

    [Fact]
    public void AuthService_Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var jwtService = new JwtTokenService("test-secret-key", 24);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new AuthService(null!, jwtService));
    }

    [Fact]
    public void AuthService_Constructor_WithNullJwtService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new AuthService(_context, null!));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ExpiryTimeIsAlways86400Seconds()
    {
        // Act
        var response1 = await _service.LoginAsync("admin@homebank.local", "demo123");
        var response2 = await _service.LoginAsync("admin@homebank.local", "demo123");

        // Assert
        Assert.Equal(86400, response1.ExpiresIn);
        Assert.Equal(86400, response2.ExpiresIn);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_TokensAreDifferent()
    {
        // Act
        var response1 = await _service.LoginAsync("admin@homebank.local", "demo123");
        var response2 = await _service.LoginAsync("admin@homebank.local", "demo123");

        // Assert - Tokens should be different (they have unique JTI claims)
        Assert.NotEqual(response1.Token, response2.Token);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_UserIdMatchesRequestedUser()
    {
        // Arrange
        var user = _context.Users.First(u => u.Email == "admin@homebank.local");

        // Act
        var response = await _service.LoginAsync("admin@homebank.local", "demo123");

        // Assert
        Assert.Equal(user.Id, response.User.Id);
    }

    [Fact]
    public async Task LoginAsync_CaseInsensitive_Email_ThrowsUnauthorizedAccessException()
    {
        // Act & Assert - Email should be case-sensitive in this implementation
        // This test verifies the current behavior - can be changed if case-insensitive is required
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync("ADMIN@HOMEBANK.LOCAL", "demo123"));
    }
}
