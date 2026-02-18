namespace HomeBanking.Tests.Controllers;

using System.Security.Claims;
using HomeBanking.API.Controllers;
using HomeBanking.API.DTOs.Requests;
using HomeBanking.API.DTOs.Responses;
using HomeBanking.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

/// <summary>
/// Unit tests for TransfersController.
/// Tests POST /api/transfers endpoint with various scenarios including success and error cases.
/// </summary>
public class TransfersControllerTests
{
    private readonly Mock<ITransferService> transferServiceMock;
    private readonly TransfersController controller;

    public TransfersControllerTests()
    {
        this.transferServiceMock = new Mock<ITransferService>();
        this.controller = new TransfersController(this.transferServiceMock.Object);
    }

    [Fact]
    public async Task PostTransfer_WithValidRequest_ReturnsCreatedWithTransferDetails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var amount = 500.00m;
        var description = "Payment to savings";

        var transferRequest = new TransferRequest
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amount,
            Description = description,
        };

        var transferResult = TransferResult.Success(
            fromAccountId,
            toAccountId,
            amount,
            4500.00m, // new balance for source
            1500.00m); // new balance for destination

        this.transferServiceMock
            .Setup(s => s.ExecuteTransferAsync(fromAccountId, toAccountId, amount, description, userId))
            .ReturnsAsync(transferResult);

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.PostTransfer(transferRequest);

        // Assert
        var createdResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);

        var response = Assert.IsType<TransferResponse>(createdResult.Value);
        Assert.Equal("completed", response.Status);
        Assert.Equal(amount, response.Amount);
        Assert.Equal(fromAccountId, response.FromAccount.Id);
        Assert.Equal(toAccountId, response.ToAccount.Id);
        Assert.Equal(4500.00m, response.FromAccount.NewBalance);
        Assert.Equal(1500.00m, response.ToAccount.NewBalance);
    }

    [Fact]
    public async Task PostTransfer_WithValidRequestAndNoDescription_ReturnsCreatedWithTransferDetails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var amount = 250.00m;

        var transferRequest = new TransferRequest
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amount,
            Description = null, // No description
        };

        var transferResult = TransferResult.Success(
            fromAccountId,
            toAccountId,
            amount,
            4750.00m,
            1250.00m);

        this.transferServiceMock
            .Setup(s => s.ExecuteTransferAsync(fromAccountId, toAccountId, amount, null, userId))
            .ReturnsAsync(transferResult);

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.PostTransfer(transferRequest);

        // Assert
        var createdResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        var response = Assert.IsType<TransferResponse>(createdResult.Value);
        Assert.NotNull(response.TransferId);
    }

    [Fact]
    public async Task PostTransfer_WithMinimumValidAmount_ReturnsCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var amount = 0.01m; // Minimum valid amount

        var transferRequest = new TransferRequest
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amount,
            Description = "Minimum transfer",
        };

        var transferResult = TransferResult.Success(
            fromAccountId,
            toAccountId,
            amount,
            4999.99m,
            10000.01m);

        this.transferServiceMock
            .Setup(s => s.ExecuteTransferAsync(fromAccountId, toAccountId, amount, "Minimum transfer", userId))
            .ReturnsAsync(transferResult);

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.PostTransfer(transferRequest);

        // Assert
        var createdResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
    }

    [Fact]
    public async Task PostTransfer_WithNullRequest_ReturnsBadRequest()
    {
        // Arrange
        this.SetupUserClaim(Guid.NewGuid());

        // Act
        var result = await this.controller.PostTransfer(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal("InvalidRequest", errorResponse.Code);
        Assert.Contains("required", errorResponse.Message.ToLower());
    }

    [Fact]
    public async Task PostTransfer_WithAmountLessThanMinimum_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transferRequest = new TransferRequest
        {
            FromAccountId = Guid.NewGuid(),
            ToAccountId = Guid.NewGuid(),
            Amount = 0.001m, // Less than minimum 0.01
            Description = "Invalid amount",
        };

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.PostTransfer(transferRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal("ValidationError", errorResponse.Code);
        Assert.Contains("greater than 0.01", errorResponse.Details?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task PostTransfer_WithZeroAmount_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transferRequest = new TransferRequest
        {
            FromAccountId = Guid.NewGuid(),
            ToAccountId = Guid.NewGuid(),
            Amount = 0m,
            Description = "Zero amount",
        };

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.PostTransfer(transferRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal("ValidationError", errorResponse.Code);
    }

    [Fact]
    public async Task PostTransfer_WithNegativeAmount_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transferRequest = new TransferRequest
        {
            FromAccountId = Guid.NewGuid(),
            ToAccountId = Guid.NewGuid(),
            Amount = -100m,
            Description = "Negative amount",
        };

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.PostTransfer(transferRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal("ValidationError", errorResponse.Code);
    }

    [Fact]
    public async Task PostTransfer_WithDescriptionTooLong_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var longDescription = new string('a', 501); // 501 characters (exceeds 500 limit)
        var transferRequest = new TransferRequest
        {
            FromAccountId = Guid.NewGuid(),
            ToAccountId = Guid.NewGuid(),
            Amount = 100m,
            Description = longDescription,
        };

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.PostTransfer(transferRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal("ValidationError", errorResponse.Code);
    }

    [Fact]
    public async Task PostTransfer_WithInsufficientBalance_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var amount = 10000.00m;

        var transferRequest = new TransferRequest
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amount,
            Description = "Too much money",
        };

        var transferResult = TransferResult.Failure(
            "InsufficientBalance",
            "Transfer amount exceeds available balance.",
            new { available = 1000.00m, requested = 10000.00m });

        this.transferServiceMock
            .Setup(s => s.ExecuteTransferAsync(fromAccountId, toAccountId, amount, "Too much money", userId))
            .ReturnsAsync(transferResult);

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.PostTransfer(transferRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal("InsufficientBalance", errorResponse.Code);
        Assert.Contains("balance", errorResponse.Message.ToLower());
    }

    [Fact]
    public async Task PostTransfer_WithAccountNotFound_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var nonExistentAccountId = Guid.NewGuid();

        var transferRequest = new TransferRequest
        {
            FromAccountId = fromAccountId,
            ToAccountId = nonExistentAccountId,
            Amount = 500.00m,
            Description = "To nonexistent account",
        };

        var transferResult = TransferResult.Failure(
            "AccountNotFound",
            "One or both accounts were not found.");

        this.transferServiceMock
            .Setup(s => s.ExecuteTransferAsync(
                fromAccountId,
                nonExistentAccountId,
                500.00m,
                "To nonexistent account",
                userId))
            .ReturnsAsync(transferResult);

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.PostTransfer(transferRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal("AccountNotFound", errorResponse.Code);
    }

    [Fact]
    public async Task PostTransfer_WithSameSourceAndDestination_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var transferRequest = new TransferRequest
        {
            FromAccountId = accountId,
            ToAccountId = accountId, // Same as source
            Amount = 500.00m,
            Description = "To same account",
        };

        var transferResult = TransferResult.Failure(
            "InvalidTransfer",
            "Source and destination accounts cannot be the same.");

        this.transferServiceMock
            .Setup(s => s.ExecuteTransferAsync(accountId, accountId, 500.00m, "To same account", userId))
            .ReturnsAsync(transferResult);

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.PostTransfer(transferRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal("InvalidTransfer", errorResponse.Code);
    }

    [Fact]
    public async Task PostTransfer_WithoutJwtToken_ReturnsUnauthorized()
    {
        // Arrange
        var transferRequest = new TransferRequest
        {
            FromAccountId = Guid.NewGuid(),
            ToAccountId = Guid.NewGuid(),
            Amount = 500.00m,
            Description = "No auth",
        };

        // No user claim setup - simulates missing JWT
        this.controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(), // Empty claims
            },
        };

        // Act
        var result = await this.controller.PostTransfer(transferRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
        Assert.Equal("Unauthorized", errorResponse.Code);
    }

    [Fact]
    public async Task PostTransfer_WithInvalidUserIdInClaim_ReturnsUnauthorized()
    {
        // Arrange
        var transferRequest = new TransferRequest
        {
            FromAccountId = Guid.NewGuid(),
            ToAccountId = Guid.NewGuid(),
            Amount = 500.00m,
            Description = "Invalid user ID",
        };

        this.controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "not-a-guid"), // Invalid GUID
                })),
            },
        };

        // Act
        var result = await this.controller.PostTransfer(transferRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
        Assert.Equal("Unauthorized", errorResponse.Code);
    }

    [Fact]
    public async Task PostTransfer_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var transferRequest = new TransferRequest
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = 500.00m,
            Description = "Service throws",
        };

        this.transferServiceMock
            .Setup(s => s.ExecuteTransferAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), userId))
            .ThrowsAsync(new ArgumentException("Invalid argument"));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.PostTransfer(transferRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal("InvalidRequest", errorResponse.Code);
    }

    [Fact]
    public async Task PostTransfer_WhenServiceThrowsUnexpectedException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        var transferRequest = new TransferRequest
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = 500.00m,
            Description = "Service throws",
        };

        this.transferServiceMock
            .Setup(s => s.ExecuteTransferAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), userId))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.PostTransfer(transferRequest);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(statusCodeResult.Value);
        Assert.Equal("InternalServerError", errorResponse.Code);
    }

    private void SetupUserClaim(Guid userId)
    {
        this.controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                })),
            },
        };
    }
}
