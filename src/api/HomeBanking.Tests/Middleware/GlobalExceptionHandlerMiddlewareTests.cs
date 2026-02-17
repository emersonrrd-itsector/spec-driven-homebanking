namespace HomeBanking.Tests.Middleware;

using System.Text.Json;
using HomeBanking.API.DTOs.Responses;
using HomeBanking.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// Unit tests for GlobalExceptionHandlerMiddleware.
/// Tests exception handling, error response formatting, logging, and HTTP status code mapping.
/// </summary>
public class GlobalExceptionHandlerMiddlewareTests
{
    [Fact]
    public async Task Invoke_WithArgumentException_Returns400BadRequest()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/transfers";
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new ArgumentException("Invalid argument"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_WithInvalidOperationException_Returns409Conflict()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/transfers";
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new InvalidOperationException("Invalid operation"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_WithKeyNotFoundException_Returns404NotFound()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/accounts/invalid-id";
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new KeyNotFoundException("Account not found"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_WithUnauthorizedAccessException_Returns401Unauthorized()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/accounts";
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new UnauthorizedAccessException("User not authorized"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_WithGenericException_Returns500InternalServerError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/accounts";
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new Exception("Unexpected error"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_ReturnsCorrectErrorCode_ForArgumentException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new ArgumentException("Invalid input"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var bodyContent = await reader.ReadToEndAsync();
        Assert.NotEmpty(bodyContent);
        Assert.Contains("BadRequest", bodyContent);
    }

    [Fact]
    public async Task Invoke_ReturnsCorrectErrorCode_ForKeyNotFoundException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new KeyNotFoundException("Not found"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var bodyContent = await reader.ReadToEndAsync();
        Assert.NotEmpty(bodyContent);
        Assert.Contains("NotFound", bodyContent);
    }

    [Fact]
    public async Task Invoke_ReturnsCorrectErrorCode_ForInvalidOperationException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new InvalidOperationException("Conflict"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var bodyContent = await reader.ReadToEndAsync();
        Assert.NotEmpty(bodyContent);
        Assert.Contains("Conflict", bodyContent);
    }

    [Fact]
    public async Task Invoke_ReturnsCorrectErrorCode_ForUnauthorizedAccessException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new UnauthorizedAccessException("Unauthorized"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var bodyContent = await reader.ReadToEndAsync();
        Assert.NotEmpty(bodyContent);
        Assert.Contains("Unauthorized", bodyContent);
    }

    [Fact]
    public async Task Invoke_ReturnsCorrectErrorCode_ForGenericException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new Exception("Unexpected"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var bodyContent = await reader.ReadToEndAsync();
        Assert.NotEmpty(bodyContent);
        Assert.Contains("InternalServerError", bodyContent);
    }

    [Fact]
    public async Task Invoke_SetsContentTypeToJson()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new Exception("Error"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.NotNull(context.Response.ContentType);
        Assert.StartsWith("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task Invoke_IncludesHumanReadableErrorMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new ArgumentException("Invalid argument"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var bodyContent = await reader.ReadToEndAsync();
        Assert.NotEmpty(bodyContent);
        Assert.Contains("invalid", bodyContent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invalid argument", bodyContent);
    }

    [Fact]
    public async Task Invoke_ArgumentException_IncludesParameterNameInDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new ArgumentException("Invalid value", "accountId"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var bodyContent = await reader.ReadToEndAsync();
        Assert.NotEmpty(bodyContent);
        Assert.Contains("accountId", bodyContent);
    }

    [Fact]
    public void Invoke_WithMiddlewareNullArgument_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(
            () => new GlobalExceptionHandlerMiddleware(null, CreateMockLogger()));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Fact]
    public void Invoke_WithLoggerNullArgument_ThrowsArgumentNullException()
    {
        // Arrange
        RequestDelegate next = async _ => await Task.CompletedTask;

        // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(
            () => new GlobalExceptionHandlerMiddleware(next, null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Fact]
    public async Task Invoke_WhenNextMiddlewareSucceeds_PassesThrough()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.StatusCode = 200;
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            ctx =>
            {
                ctx.Response.StatusCode = 200;
                return Task.CompletedTask;
            },
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_LogsExceptionWithRequestContext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/transfers";
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new Exception("Test error"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_ReturnsGenericErrorMessageOnCriticalException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new OutOfMemoryException("Out of memory"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var bodyContent = await reader.ReadToEndAsync();
        Assert.NotEmpty(bodyContent);
        Assert.Contains("InternalServerError", bodyContent);
    }

    [Fact]
    public async Task Invoke_KeyNotFoundException_ReturnsNotFoundMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new KeyNotFoundException("Account not found"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var bodyContent = await reader.ReadToEndAsync();
        Assert.NotEmpty(bodyContent);
        Assert.Contains("NotFound", bodyContent);
        Assert.Contains("requested resource", bodyContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Invoke_UnauthorizedAccessException_ReturnsUnauthorizedMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new UnauthorizedAccessException("Not authorized"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var bodyContent = await reader.ReadToEndAsync();
        Assert.NotEmpty(bodyContent);
        Assert.Contains("Unauthorized", bodyContent);
        Assert.Contains("not authorized", bodyContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Invoke_ErrorResponseIncludesTimestamp()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var loggerMock = CreateMockLogger();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new Exception("Error"),
            loggerMock);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var bodyContent = await reader.ReadToEndAsync();
        Assert.NotEmpty(bodyContent);
        Assert.Contains("timestamp", bodyContent, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Helper method to create a mock logger.
    /// </summary>
    /// <returns>A logger instance for testing.</returns>
    private static ILogger<GlobalExceptionHandlerMiddleware> CreateMockLogger()
    {
        return new TestLogger();
    }

    /// <summary>
    /// A simple test logger implementation that does nothing (suitable for middleware testing).
    /// </summary>
    private class TestLogger : ILogger<GlobalExceptionHandlerMiddleware>
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            // No-op for testing
        }
    }
}
