namespace HomeBanking.Tests.Controllers;

using HomeBanking.API.Controllers;
using HomeBanking.API.Data;
using HomeBanking.API.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Unit tests for HealthController.
/// Tests health check and readiness endpoints.
/// </summary>
public class HealthControllerTests
{
    [Fact]
    public void Health_ReturnsOkStatusCode()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Health();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public void Health_ReturnsHealthyStatus()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Health();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HealthResponse>(okResult.Value);
        Assert.Equal("healthy", response.Status);
    }

    [Fact]
    public void Health_ReturnsCorrectVersion()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Health();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HealthResponse>(okResult.Value);
        Assert.Equal("1.0.0", response.Version);
    }

    [Fact]
    public void Health_ReturnsValidTimestamp()
    {
        // Arrange
        var beforeCall = DateTime.UtcNow;
        var controller = CreateController();

        // Act
        var result = controller.Health();
        var afterCall = DateTime.UtcNow;

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HealthResponse>(okResult.Value);
        Assert.True(response.Timestamp >= beforeCall && response.Timestamp <= afterCall);
    }

    [Fact]
    public void Health_ReturnsUtcTimestamp()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Health();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HealthResponse>(okResult.Value);
        Assert.Equal(DateTimeKind.Utc, response.Timestamp.Kind);
    }

    [Fact]
    public async Task Ready_WithHealthyDatabase_ReturnsOkStatusCode()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Ready();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task Ready_WithHealthyDatabase_ReturnsReadyStatus()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Ready();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ReadinessResponse>(okResult.Value);
        Assert.Equal("ready", response.Status);
    }

    [Fact]
    public async Task Ready_WithHealthyDatabase_IncludesTimestamp()
    {
        // Arrange
        var beforeCall = DateTime.UtcNow;
        var controller = CreateController();

        // Act
        var result = await controller.Ready();
        var afterCall = DateTime.UtcNow;

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ReadinessResponse>(okResult.Value);
        Assert.True(response.Timestamp >= beforeCall && response.Timestamp <= afterCall);
    }

    [Fact]
    public async Task Ready_WithHealthyDatabase_IncludesDetails()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Ready();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ReadinessResponse>(okResult.Value);
        Assert.NotNull(response.Details);
    }

    [Fact]
    public async Task Ready_ReturnsUtcTimestamp()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Ready();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ReadinessResponse>(okResult.Value);
        Assert.Equal(DateTimeKind.Utc, response.Timestamp.Kind);
    }

    [Fact]
    public void HealthController_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => new HealthController(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    private static HealthController CreateController()
    {
        var options = new DbContextOptionsBuilder<HomeBankingContext>()
            .UseInMemoryDatabase("TestHealthDb")
            .Options;

        var context = new HomeBankingContext(options);
        return new HealthController(context);
    }
}
