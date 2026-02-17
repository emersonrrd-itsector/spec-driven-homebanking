namespace HomeBanking.Tests.Controllers;

using System.Security.Claims;
using HomeBanking.API.Controllers;
using HomeBanking.API.DTOs.Responses;
using HomeBanking.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

/// <summary>
/// Unit tests for AccountsController.
/// Tests GetAccounts() and GetAccount(id) endpoints.
/// </summary>
public class AccountsControllerTests
{
    private readonly Mock<IAccountService> accountServiceMock;
    private readonly AccountsController controller;

    public AccountsControllerTests()
    {
        this.accountServiceMock = new Mock<IAccountService>();
        this.controller = new AccountsController(this.accountServiceMock.Object);
    }

    [Fact]
    public async Task GetAccounts_WithValidToken_ReturnsOkWithAccounts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accounts = new List<AccountDto>
        {
            new AccountDto
            {
                Id = Guid.NewGuid(),
                Name = "Checking",
                Balance = 5000.00m,
                Currency = "USD",
                LastUpdated = DateTime.UtcNow,
            },
            new AccountDto
            {
                Id = Guid.NewGuid(),
                Name = "Savings",
                Balance = 15000.00m,
                Currency = "USD",
                LastUpdated = DateTime.UtcNow,
            },
        };

        this.accountServiceMock
            .Setup(s => s.GetUserAccountsAsync(userId))
            .ReturnsAsync(accounts);

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetAccounts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        var response = Assert.IsType<AccountsResponse>(okResult.Value);
        Assert.Equal(2, response.Accounts.Count());
    }

    [Fact]
    public async Task GetAccounts_WithValidToken_ReturnsAllAccountsForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account1Id = Guid.NewGuid();
        var account2Id = Guid.NewGuid();
        var accounts = new List<AccountDto>
        {
            new AccountDto
            {
                Id = account1Id,
                Name = "Checking",
                Balance = 5000.00m,
                Currency = "USD",
                LastUpdated = DateTime.UtcNow,
            },
            new AccountDto
            {
                Id = account2Id,
                Name = "Savings",
                Balance = 15000.00m,
                Currency = "USD",
                LastUpdated = DateTime.UtcNow,
            },
        };

        this.accountServiceMock
            .Setup(s => s.GetUserAccountsAsync(userId))
            .ReturnsAsync(accounts);

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetAccounts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AccountsResponse>(okResult.Value);
        var accountList = response.Accounts.ToList();
        Assert.Equal(2, accountList.Count);
        Assert.Equal(account1Id, accountList[0].Id);
        Assert.Equal(account2Id, accountList[1].Id);
    }

    [Fact]
    public async Task GetAccounts_WithValidTokenAndNoAccounts_ReturnsOkWithEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accounts = new List<AccountDto>();

        this.accountServiceMock
            .Setup(s => s.GetUserAccountsAsync(userId))
            .ReturnsAsync(accounts);

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetAccounts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AccountsResponse>(okResult.Value);
        Assert.Empty(response.Accounts);
    }

    [Fact]
    public async Task GetAccounts_WithoutUserIdClaim_ReturnsUnauthorized()
    {
        // Arrange
        this.controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { })),
            },
        };

        // Act
        var result = await this.controller.GetAccounts();

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
        Assert.Equal("Unauthorized", errorResponse.Code);
    }

    [Fact]
    public async Task GetAccounts_WithInvalidUserIdClaim_ReturnsUnauthorized()
    {
        // Arrange
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
        var result = await this.controller.GetAccounts();

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task GetAccounts_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        this.accountServiceMock
            .Setup(s => s.GetUserAccountsAsync(userId))
            .ThrowsAsync(new Exception("Database error"));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetAccounts();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(statusResult.Value);
        Assert.Equal("InternalServerError", errorResponse.Code);
    }

    [Fact]
    public async Task GetAccounts_WithValidToken_CallsServiceOnce()
    {
        // Arrange
        var userId = Guid.NewGuid();

        this.accountServiceMock
            .Setup(s => s.GetUserAccountsAsync(userId))
            .ReturnsAsync(new List<AccountDto>());

        this.SetupUserClaim(userId);

        // Act
        await this.controller.GetAccounts();

        // Assert
        this.accountServiceMock.Verify(
            s => s.GetUserAccountsAsync(userId),
            Times.Once);
    }

    [Fact]
    public async Task GetAccount_WithValidIdAndToken_ReturnsOkWithAccount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var account = new AccountDto
        {
            Id = accountId,
            Name = "Checking",
            Balance = 5000.00m,
            Currency = "USD",
            LastUpdated = DateTime.UtcNow,
        };

        this.accountServiceMock
            .Setup(s => s.GetUserAccountAsync(userId, accountId))
            .ReturnsAsync(account);

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetAccount(accountId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedAccount = Assert.IsType<AccountDto>(okResult.Value);
        Assert.Equal(accountId, returnedAccount.Id);
        Assert.Equal("Checking", returnedAccount.Name);
    }

    [Fact]
    public async Task GetAccount_WithValidIdAndToken_ReturnsAccountDetails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var lastUpdated = DateTime.UtcNow;
        var account = new AccountDto
        {
            Id = accountId,
            Name = "Savings",
            Balance = 15000.00m,
            Currency = "USD",
            LastUpdated = lastUpdated,
        };

        this.accountServiceMock
            .Setup(s => s.GetUserAccountAsync(userId, accountId))
            .ReturnsAsync(account);

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetAccount(accountId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedAccount = Assert.IsType<AccountDto>(okResult.Value);
        Assert.Equal("Savings", returnedAccount.Name);
        Assert.Equal(15000.00m, returnedAccount.Balance);
        Assert.Equal("USD", returnedAccount.Currency);
    }

    [Fact]
    public async Task GetAccount_WithNonexistentAccountId_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        this.accountServiceMock
            .Setup(s => s.GetUserAccountAsync(userId, accountId))
            .ReturnsAsync((AccountDto?)null);

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetAccount(accountId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(notFoundResult.Value);
        Assert.Equal("AccountNotFound", errorResponse.Code);
    }

    [Fact]
    public async Task GetAccount_WithAccountFromDifferentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        this.accountServiceMock
            .Setup(s => s.GetUserAccountAsync(userId, accountId))
            .ReturnsAsync((AccountDto?)null);

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetAccount(accountId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetAccount_WithoutUserIdClaim_ReturnsUnauthorized()
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
        var result = await this.controller.GetAccount(accountId);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task GetAccount_WithInvalidUserIdClaim_ReturnsUnauthorized()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        this.controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "not-a-guid"),
                })),
            },
        };

        // Act
        var result = await this.controller.GetAccount(accountId);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task GetAccount_WithEmptyAccountId_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.Empty;

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetAccount(accountId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal("InvalidRequest", errorResponse.Code);
    }

    [Fact]
    public async Task GetAccount_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        this.accountServiceMock
            .Setup(s => s.GetUserAccountAsync(userId, accountId))
            .ThrowsAsync(new Exception("Database error"));

        this.SetupUserClaim(userId);

        // Act
        var result = await this.controller.GetAccount(accountId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(statusResult.Value);
        Assert.Equal("InternalServerError", errorResponse.Code);
    }

    [Fact]
    public async Task GetAccount_WithValidId_CallsServiceOnce()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        this.accountServiceMock
            .Setup(s => s.GetUserAccountAsync(userId, accountId))
            .ReturnsAsync((AccountDto?)null);

        this.SetupUserClaim(userId);

        // Act
        await this.controller.GetAccount(accountId);

        // Assert
        this.accountServiceMock.Verify(
            s => s.GetUserAccountAsync(userId, accountId),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithNullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new AccountsController(null!));
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
