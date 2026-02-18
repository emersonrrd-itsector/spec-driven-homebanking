namespace HomeBanking.Tests.Controllers;

using HomeBanking.API.Controllers;
using HomeBanking.API.DTOs.Requests;
using HomeBanking.API.DTOs.Responses;
using HomeBanking.API.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

/// <summary>
/// Unit tests for AuthController.
/// </summary>
public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Login_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "admin@homebank.local", Password = "demo123" };
        var loginResponse = new LoginResponse
        {
            Token = "test-token",
            ExpiresIn = 86400,
            User = new UserDto { Id = Guid.NewGuid(), Email = "admin@homebank.local" },
        };

        _authServiceMock
            .Setup(s => s.LoginAsync(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedResponse = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal(loginResponse.Token, returnedResponse.Token);
    }

    [Fact]
    public async Task Login_WithValidRequest_ReturnsLoginResponse()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "admin@homebank.local", Password = "demo123" };
        var expectedResponse = new LoginResponse
        {
            Token = "test-token-123",
            ExpiresIn = 86400,
            User = new UserDto { Id = Guid.NewGuid(), Email = "admin@homebank.local" },
        };

        _authServiceMock
            .Setup(s => s.LoginAsync(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal("test-token-123", response.Token);
        Assert.Equal(86400, response.ExpiresIn);
        Assert.Equal("admin@homebank.local", response.User.Email);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorizedResult()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "admin@homebank.local", Password = "wrongpassword" };

        _authServiceMock
            .Setup(s => s.LoginAsync(loginRequest.Email, loginRequest.Password))
            .ThrowsAsync(new UnauthorizedAccessException("Email or password incorrect."));

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
        Assert.Equal("InvalidCredentials", errorResponse.Code);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsErrorResponse()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "admin@homebank.local", Password = "wrongpassword" };

        _authServiceMock
            .Setup(s => s.LoginAsync(loginRequest.Email, loginRequest.Password))
            .ThrowsAsync(new UnauthorizedAccessException("Email or password incorrect."));

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
        Assert.Equal("InvalidCredentials", errorResponse.Code);
        Assert.Equal("Email or password incorrect.", errorResponse.Message);
        Assert.NotEqual(DateTime.MinValue, errorResponse.Timestamp);
    }

    [Fact]
    public async Task Login_WithNullRequest_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Login(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal("InvalidRequest", errorResponse.Code);
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = string.Empty, Password = "demo123" };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "admin@homebank.local", Password = string.Empty };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithNullEmail_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = null!, Password = "demo123" };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithNullPassword_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "admin@homebank.local", Password = null! };

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "admin@homebank.local", Password = "demo123" };

        _authServiceMock
            .Setup(s => s.LoginAsync(loginRequest.Email, loginRequest.Password))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(statusResult.Value);
        Assert.Equal("InternalServerError", errorResponse.Code);
    }

    [Fact]
    public void AuthController_Constructor_WithNullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new AuthController(null!));
    }

    [Fact]
    public async Task Login_WithValidRequest_CallsAuthServiceLoginAsyncOnce()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "admin@homebank.local", Password = "demo123" };
        var loginResponse = new LoginResponse
        {
            Token = "test-token",
            ExpiresIn = 86400,
            User = new UserDto { Id = Guid.NewGuid(), Email = "admin@homebank.local" },
        };

        _authServiceMock
            .Setup(s => s.LoginAsync(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync(loginResponse);

        // Act
        await _controller.Login(loginRequest);

        // Assert
        _authServiceMock.Verify(
            s => s.LoginAsync(loginRequest.Email, loginRequest.Password),
            Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidRequest_DoesNotCallAuthService()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = string.Empty, Password = "demo123" };

        // Act
        await _controller.Login(loginRequest);

        // Assert
        _authServiceMock.Verify(
            s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_WithValidRequest_ErrorResponseHasTimestamp()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "admin@homebank.local", Password = "wrongpassword" };

        _authServiceMock
            .Setup(s => s.LoginAsync(loginRequest.Email, loginRequest.Password))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
        Assert.NotEqual(DateTime.MinValue, errorResponse.Timestamp);
        Assert.True(errorResponse.Timestamp.Kind == DateTimeKind.Utc);
    }
}
