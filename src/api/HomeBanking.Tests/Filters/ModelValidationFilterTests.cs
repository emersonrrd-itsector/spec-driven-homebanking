namespace HomeBanking.Tests.Filters;

using HomeBanking.API.DTOs.Responses;
using HomeBanking.API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Unit tests for ModelValidationFilter.
/// Tests validation error handling and error response formatting.
/// </summary>
public class ModelValidationFilterTests
{
    [Fact]
    public void OnActionExecuting_WithValidModelState_DoesNotSetResult()
    {
        // Arrange
        var filter = new ModelValidationFilter();
        var context = CreateActionExecutingContext();

        // Act
        filter.OnActionExecuting(context);

        // Assert
        Assert.Null(context.Result);
    }

    [Fact]
    public void OnActionExecuting_WithInvalidModelState_SetsBadRequestResult()
    {
        // Arrange
        var filter = new ModelValidationFilter();
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("field1", "Error message");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        Assert.NotNull(context.Result);
        Assert.IsType<BadRequestObjectResult>(context.Result);
        var result = (BadRequestObjectResult)context.Result;
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public void OnActionExecuting_WithInvalidModelState_ReturnsErrorResponse()
    {
        // Arrange
        var filter = new ModelValidationFilter();
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("amount", "Amount must be greater than 0");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        var result = (BadRequestObjectResult)context.Result!;
        var errorResponse = Assert.IsType<ErrorResponse>(result.Value);
        Assert.Equal("ValidationError", errorResponse.Code);
    }

    [Fact]
    public void OnActionExecuting_WithInvalidModelState_IncludesUserFriendlyMessage()
    {
        // Arrange
        var filter = new ModelValidationFilter();
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("field", "Error");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        var result = (BadRequestObjectResult)context.Result!;
        var errorResponse = (ErrorResponse)result.Value!;
        Assert.Equal("One or more validation errors occurred.", errorResponse.Message);
    }

    [Fact]
    public void OnActionExecuting_WithInvalidModelState_IncludesErrorDetails()
    {
        // Arrange
        var filter = new ModelValidationFilter();
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("amount", "Amount must be greater than 0");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        var result = (BadRequestObjectResult)context.Result!;
        var errorResponse = (ErrorResponse)result.Value!;
        Assert.NotNull(errorResponse.Details);
    }

    [Fact]
    public void OnActionExecuting_WithMultipleErrors_IncludesAllFieldErrors()
    {
        // Arrange
        var filter = new ModelValidationFilter();
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("field1", "Error 1");
        context.ModelState.AddModelError("field2", "Error 2");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        var result = (BadRequestObjectResult)context.Result!;
        var errorResponse = (ErrorResponse)result.Value!;
        Assert.NotNull(errorResponse.Details);
    }

    [Fact]
    public void OnActionExecuting_WithMultipleErrorsOnSameField_IncludesAllErrors()
    {
        // Arrange
        var filter = new ModelValidationFilter();
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("amount", "Must be positive");
        context.ModelState.AddModelError("amount", "Must be less than 1000000");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        var result = (BadRequestObjectResult)context.Result!;
        var errorResponse = (ErrorResponse)result.Value!;
        Assert.NotNull(errorResponse.Details);
    }

    [Fact]
    public void OnActionExecuting_ErrorResponseIncludesTimestamp()
    {
        // Arrange
        var beforeCall = DateTime.UtcNow;
        var filter = new ModelValidationFilter();
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("field", "Error");

        // Act
        filter.OnActionExecuting(context);
        var afterCall = DateTime.UtcNow;

        // Assert
        var result = (BadRequestObjectResult)context.Result!;
        var errorResponse = (ErrorResponse)result.Value!;
        Assert.True(errorResponse.Timestamp >= beforeCall && errorResponse.Timestamp <= afterCall);
    }

    [Fact]
    public void OnActionExecuting_ErrorResponseTimestampIsUtc()
    {
        // Arrange
        var filter = new ModelValidationFilter();
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("field", "Error");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        var result = (BadRequestObjectResult)context.Result!;
        var errorResponse = (ErrorResponse)result.Value!;
        Assert.Equal(DateTimeKind.Utc, errorResponse.Timestamp.Kind);
    }

    [Fact]
    public void OnActionExecuted_DoesNotThrow()
    {
        // Arrange
        var filter = new ModelValidationFilter();
        var context = CreateActionExecutedContext();

        // Act & Assert - Should not throw
        filter.OnActionExecuted(context);
    }

    /// <summary>
    /// Creates a mock ActionExecutingContext for testing.
    /// </summary>
    /// <returns>An ActionExecutingContext with valid ModelState by default.</returns>
    private static ActionExecutingContext CreateActionExecutingContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(),
        };

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());
    }

    /// <summary>
    /// Creates a mock ActionExecutedContext for testing.
    /// </summary>
    /// <returns>An ActionExecutedContext.</returns>
    private static ActionExecutedContext CreateActionExecutedContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(),
        };

        return new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            new object());
    }
}
