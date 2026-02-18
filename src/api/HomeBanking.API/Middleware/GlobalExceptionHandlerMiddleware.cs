namespace HomeBanking.API.Middleware;

using System.Text.Json;
using HomeBanking.API.DTOs.Responses;

/// <summary>
/// Global exception handler middleware that catches all unhandled exceptions in the request pipeline.
/// Converts exceptions to standardized ErrorResponse format with appropriate HTTP status codes.
/// Logs all exceptions with request context (timestamp, path, method, status code).
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandlerMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance for recording exceptions.</param>
    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        this.next = next ?? throw new ArgumentNullException(nameof(next));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware to process the HTTP request.
    /// Catches any unhandled exceptions and converts them to ErrorResponse with appropriate status codes.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this.next(context);
        }
        catch (Exception exception)
        {
            await this.HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// Maps an exception type to an appropriate HTTP status code and error code.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>A tuple containing the HTTP status code and error code.</returns>
    private static (int StatusCode, string ErrorCode) MapExceptionToStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "BadRequest"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "NotFound"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _ => (StatusCodes.Status500InternalServerError, "InternalServerError"),
        };
    }

    /// <summary>
    /// Gets a user-friendly error message based on the exception and error code.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="errorCode">The error code.</param>
    /// <returns>A user-friendly error message.</returns>
    private static string GetErrorMessage(Exception exception, string errorCode)
    {
        return errorCode switch
        {
            "BadRequest" => "The request contains invalid data. Please check your input and try again.",
            "Conflict" => "The operation could not be completed due to a conflict. Please try again.",
            "NotFound" => "The requested resource was not found.",
            "Unauthorized" => "You are not authorized to perform this action.",
            "InternalServerError" => "An unexpected error occurred while processing your request.",
            _ => exception.Message ?? "An error occurred.",
        };
    }

    /// <summary>
    /// Extracts error details from the exception if available.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>An object containing error details, or null if no additional details are available.</returns>
    private static object? GetErrorDetails(Exception exception)
    {
        // For general exceptions, we don't expose internal details in production-like scenarios
        // In development, we might want to include the exception message or stack trace
        // For now, we return null to keep details focused on business logic errors

        // If the exception is an ArgumentException, include the parameter name
        if (exception is ArgumentException argEx)
        {
            return new
            {
                paramName = argEx.ParamName,
            };
        }

        return null;
    }

    /// <summary>
    /// Handles the conversion of an exception to a standardized error response.
    /// Maps specific exception types to appropriate HTTP status codes.
    /// Logs the exception with full context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Never throw in exception handler - be defensive
        try
        {
            var timestamp = DateTime.UtcNow;
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;

            // Map exception to HTTP status code and error code
            var (statusCode, errorCode) = MapExceptionToStatusCode(exception);

            // Log the exception with full context
            this.logger.LogError(
                exception,
                "Unhandled exception caught. Path: {RequestPath}, Method: {RequestMethod}, StatusCode: {StatusCode}, ErrorCode: {ErrorCode}, Timestamp: {Timestamp}",
                requestPath,
                requestMethod,
                statusCode,
                errorCode,
                timestamp);

            // Create standardized error response
            var errorResponse = new ErrorResponse
            {
                Code = errorCode,
                Message = GetErrorMessage(exception, errorCode),
                Details = GetErrorDetails(exception),
                Timestamp = timestamp,
            };

            // Set response properties
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsJsonAsync(errorResponse);
        }
        catch (Exception handlingException)
        {
            // If an error occurs during exception handling, log it and write a generic error response
            this.logger.LogCritical(
                handlingException,
                "Critical error occurred while handling exception. Original exception: {OriginalException}",
                exception.GetType().Name);

            // Try to return a generic 500 error
            try
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var fallbackResponse = new ErrorResponse
                {
                    Code = "InternalServerError",
                    Message = "An unexpected error occurred while processing your request.",
                    Details = null,
                    Timestamp = DateTime.UtcNow,
                };

                return context.Response.WriteAsJsonAsync(fallbackResponse);
            }
            catch
            {
                // If we can't even write the response, there's nothing more we can do
                return Task.CompletedTask;
            }
        }
    }
}
