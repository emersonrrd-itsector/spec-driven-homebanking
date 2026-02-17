namespace HomeBanking.API.Filters;

using HomeBanking.API.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

/// <summary>
/// Action filter that intercepts validation errors and returns a standardized ErrorResponse.
/// Converts ModelState errors to the consistent error format: { code, message, details, timestamp }.
/// </summary>
public class ModelValidationFilter : IActionFilter
{
    /// <summary>
    /// Called before the action method executes.
    /// Checks ModelState and returns validation errors if present.
    /// </summary>
    /// <param name="context">The action executing context.</param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            var errorResponse = new ErrorResponse
            {
                Code = "ValidationError",
                Message = "One or more validation errors occurred.",
                Details = new { errors },
                Timestamp = DateTime.UtcNow,
            };

            context.Result = new BadRequestObjectResult(errorResponse);
        }
    }

    /// <summary>
    /// Called after the action method executes.
    /// This implementation does nothing.
    /// </summary>
    /// <param name="context">The action executed context.</param>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No operation needed after action execution
    }
}
