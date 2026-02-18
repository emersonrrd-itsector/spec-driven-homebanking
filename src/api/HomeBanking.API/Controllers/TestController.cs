using HomeBanking.API.Data;
using Microsoft.AspNetCore.Mvc;

namespace HomeBanking.API.Controllers;

/// <summary>
/// Controller for testing purposes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly HomeBankingContext context;
    private readonly IWebHostEnvironment env;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestController"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="env">The web host environment.</param>
    public TestController(HomeBankingContext context, IWebHostEnvironment env)
    {
        this.context = context;
        this.env = env;
    }

    /// <summary>
    /// Resets the database to a clean state.
    /// Only available in Development environment.
    /// </summary>
    /// <returns>A status message.</returns>
    [HttpPost("reset")]
    public IActionResult ResetDatabase()
    {
        if (!this.env.IsDevelopment())
        {
            return this.NotFound();
        }

        // Clear existing data
        this.context.Transactions.RemoveRange(this.context.Transactions);
        this.context.Accounts.RemoveRange(this.context.Accounts);
        this.context.Users.RemoveRange(this.context.Users);
        this.context.SaveChanges();

        // Reseed
        this.context.SeedIfEmpty();

        return this.Ok(new { message = "Database reset successfully" });
    }
}
