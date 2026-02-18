namespace HomeBanking.Tests.Controllers;

using System.Security.Claims;
using HomeBanking.API.Controllers;
using HomeBanking.API.DTOs.Responses;
using HomeBanking.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

/// <summary>
/// Unit tests for TransactionsController.
/// Tests GetTransactions() endpoint for pagination, filtering, and authorization.
/// </summary>
public class TransactionsControllerTests
{
    private readonly Mock<ITransactionService> transactionServiceMock;
    private readonly TransactionsController controller;

    public TransactionsControllerTests()
    {
        this.transactionServiceMock = new Mock<ITransactionService>();
        this.controller = new TransactionsController(this.transactionServiceMock.Object);
    }

    [Fact]
    public async Task GetTransactions_WithValidTokenAndAccountId_ReturnsOkWithTransactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var transactions = new List<TransactionDto>
        {
            new TransactionDto
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 50.00m,
                Date = DateTime.UtcNow.AddDays(-1),
                Description = "Coffee Shop",
                Category = "Food & Dining",
                Type = "Debit",
            },
            new TransactionDto
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 2000.00m,
                Date = DateTime.UtcNow,
                Description = "Salary Deposit",
                Category = "Salary",
                Type = "Credit",
            },
        };

        this.transactionServiceMock
            .Setup(s => s.GetAccountTransactionsAsync(userId, accountId, 0, 10, null))
            .ReturnsAsync((transactions, transactions.Count));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetTransactions(accountId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        var response = Assert.IsType<TransactionsResponse>(okResult.Value);
        Assert.Equal(2, response.Transactions.Count());
        Assert.Equal(2, response.Total);
        Assert.Equal(0, response.Skip);
        Assert.Equal(10, response.Take);
    }

    [Fact]
    public async Task GetTransactions_WithValidTokenAndAccountId_ReturnsAllTransactionFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var transactions = new List<TransactionDto>
        {
            new TransactionDto
            {
                Id = transactionId,
                AccountId = accountId,
                Amount = 123.45m,
                Date = date,
                Description = "Test Transaction",
                Category = "Groceries",
                Type = "Debit",
            },
        };

        this.transactionServiceMock
            .Setup(s => s.GetAccountTransactionsAsync(userId, accountId, 0, 10, null))
            .ReturnsAsync((transactions, 1));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetTransactions(accountId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TransactionsResponse>(okResult.Value);
        var transaction = response.Transactions.First();

        Assert.Equal(transactionId, transaction.Id);
        Assert.Equal(accountId, transaction.AccountId);
        Assert.Equal(123.45m, transaction.Amount);
        Assert.Equal(date, transaction.Date);
        Assert.Equal("Test Transaction", transaction.Description);
        Assert.Equal("Groceries", transaction.Category);
        Assert.Equal("Debit", transaction.Type);
    }

    [Fact]
    public async Task GetTransactions_WithEmptyResult_ReturnsOkWithEmptyTransactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        this.transactionServiceMock
            .Setup(s => s.GetAccountTransactionsAsync(userId, accountId, 0, 10, null))
            .ReturnsAsync((new List<TransactionDto>(), 0));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetTransactions(accountId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TransactionsResponse>(okResult.Value);
        Assert.Empty(response.Transactions);
        Assert.Equal(0, response.Total);
    }

    [Fact]
    public async Task GetTransactions_WithCustomSkipAndTake_PassesValuesToService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var skip = 20;
        var take = 25;

        this.transactionServiceMock
            .Setup(s => s.GetAccountTransactionsAsync(userId, accountId, skip, take, null))
            .ReturnsAsync((new List<TransactionDto>(), 100));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetTransactions(accountId, skip, take);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TransactionsResponse>(okResult.Value);
        Assert.Equal(skip, response.Skip);
        Assert.Equal(take, response.Take);
        this.transactionServiceMock.Verify(
            s => s.GetAccountTransactionsAsync(userId, accountId, skip, take, null),
            Times.Once);
    }

    [Fact]
    public async Task GetTransactions_WithDefaultPagination_UsesSkip0Take10()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        this.transactionServiceMock
            .Setup(s => s.GetAccountTransactionsAsync(userId, accountId, 0, 10, null))
            .ReturnsAsync((new List<TransactionDto>(), 42));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetTransactions(accountId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TransactionsResponse>(okResult.Value);
        Assert.Equal(0, response.Skip);
        Assert.Equal(10, response.Take);
        Assert.Equal(42, response.Total);
    }

    [Fact]
    public async Task GetTransactions_WithPaginationValues_IncludesPaginationInResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var transactions = Enumerable.Range(1, 5)
            .Select(i => new TransactionDto
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 100m * i,
                Date = DateTime.UtcNow.AddDays(-i),
                Description = $"Transaction {i}",
                Category = "Other",
                Type = "Debit",
            })
            .ToList();

        this.transactionServiceMock
            .Setup(s => s.GetAccountTransactionsAsync(userId, accountId, 10, 5, null))
            .ReturnsAsync((transactions, 100));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetTransactions(accountId, skip: 10, take: 5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TransactionsResponse>(okResult.Value);
        Assert.Equal(5, response.Transactions.Count());
        Assert.Equal(100, response.Total);
        Assert.Equal(10, response.Skip);
        Assert.Equal(5, response.Take);
    }

    [Fact]
    public async Task GetTransactions_WithCategoryFilter_PassesCategoryToService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var category = "Groceries";
        var transactions = new List<TransactionDto>
        {
            new TransactionDto
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 50.00m,
                Date = DateTime.UtcNow,
                Description = "Grocery Store",
                Category = category,
                Type = "Debit",
            },
        };

        this.transactionServiceMock
            .Setup(s => s.GetAccountTransactionsAsync(userId, accountId, 0, 10, category))
            .ReturnsAsync((transactions, 1));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetTransactions(accountId, category: category);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TransactionsResponse>(okResult.Value);
        Assert.Single(response.Transactions);
        Assert.Equal(category, response.Transactions.First().Category);
        this.transactionServiceMock.Verify(
            s => s.GetAccountTransactionsAsync(userId, accountId, 0, 10, category),
            Times.Once);
    }

    [Fact]
    public async Task GetTransactions_WithCategoryFilter_ReturnsOnlyFilteredTransactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var utilities = new List<TransactionDto>
        {
            new TransactionDto
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 120.00m,
                Date = DateTime.UtcNow,
                Description = "Electric Bill",
                Category = "Utilities",
                Type = "Debit",
            },
        };

        this.transactionServiceMock
            .Setup(s => s.GetAccountTransactionsAsync(userId, accountId, 0, 10, "Utilities"))
            .ReturnsAsync((utilities, 1));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetTransactions(accountId, category: "Utilities");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TransactionsResponse>(okResult.Value);
        Assert.Single(response.Transactions);
        Assert.All(response.Transactions, t => Assert.Equal("Utilities", t.Category));
    }

    [Fact]
    public async Task GetTransactions_WithNoCategoryMatches_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        this.transactionServiceMock
            .Setup(s => s.GetAccountTransactionsAsync(userId, accountId, 0, 10, "NonexistentCategory"))
            .ReturnsAsync((new List<TransactionDto>(), 0));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetTransactions(accountId, category: "NonexistentCategory");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TransactionsResponse>(okResult.Value);
        Assert.Empty(response.Transactions);
        Assert.Equal(0, response.Total);
    }

    [Fact]
    public async Task GetTransactions_WithoutUserIdClaim_ReturnsUnauthorized()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        this.controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { })),
            },
        };

        // Act
        var result = await this.controller.GetTransactions(accountId);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
        Assert.Equal("Unauthorized", errorResponse.Code);
    }

    [Fact]
    public async Task GetTransactions_WithInvalidUserIdClaim_ReturnsUnauthorized()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        this.controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "invalid-guid"),
                })),
            },
        };

        // Act
        var result = await this.controller.GetTransactions(accountId);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task GetTransactions_WithEmptyAccountId_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.Empty;

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetTransactions(accountId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal("InvalidRequest", errorResponse.Code);
    }

    [Fact]
    public async Task GetTransactions_WithInvalidSkipValue_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        this.transactionServiceMock
            .Setup(s => s.GetAccountTransactionsAsync(userId, accountId, -1, 10, null))
            .ThrowsAsync(new ArgumentException("Skip value cannot be negative."));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetTransactions(accountId, skip: -1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal("InvalidRequest", errorResponse.Code);
    }

    [Fact]
    public async Task GetTransactions_WithInvalidTakeValue_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        this.transactionServiceMock
            .Setup(s => s.GetAccountTransactionsAsync(userId, accountId, 0, 101, null))
            .ThrowsAsync(new ArgumentException("Take value must be between 1 and 100."));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetTransactions(accountId, take: 101);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetTransactions_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        this.transactionServiceMock
            .Setup(s => s.GetAccountTransactionsAsync(userId, accountId, 0, 10, null))
            .ThrowsAsync(new Exception("Database error"));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetTransactions(accountId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(statusResult.Value);
        Assert.Equal("InternalServerError", errorResponse.Code);
    }

    [Fact]
    public async Task GetTransactions_OnException_IncludesErrorMessage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        this.transactionServiceMock
            .Setup(s => s.GetAccountTransactionsAsync(userId, accountId, 0, 10, null))
            .ThrowsAsync(new Exception("Timeout"));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetTransactions(accountId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorResponse>(statusResult.Value);
        Assert.NotNull(errorResponse.Details);
        var detailsString = errorResponse.Details?.ToString() ?? string.Empty;
        Assert.Contains("Timeout", detailsString);
    }

    [Fact]
    public async Task GetTransactions_WithValidToken_CallsServiceOnce()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        this.transactionServiceMock
            .Setup(s => s.GetAccountTransactionsAsync(userId, accountId, 0, 10, null))
            .ReturnsAsync((new List<TransactionDto>(), 0));

        this.SetupUserClaim(userId);

        // Act
        await this.controller.GetTransactions(accountId);

        // Assert
        this.transactionServiceMock.Verify(
            s => s.GetAccountTransactionsAsync(userId, accountId, 0, 10, null),
            Times.Once);
    }

    [Fact]
    public async Task GetTransactions_WithCategoryAndPagination_PassesAllParametersToService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var skip = 5;
        var take = 15;
        var category = "Food & Dining";

        this.transactionServiceMock
            .Setup(s => s.GetAccountTransactionsAsync(userId, accountId, skip, take, category))
            .ReturnsAsync((new List<TransactionDto>(), 0));

        this.SetupUserClaim(userId);

        // Act
        await this.controller.GetTransactions(accountId, skip, take, category);

        // Assert
        this.transactionServiceMock.Verify(
            s => s.GetAccountTransactionsAsync(userId, accountId, skip, take, category),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithNullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new TransactionsController(null!));
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
