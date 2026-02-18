using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBanking.API.Data;

/// <summary>
/// Extension methods for configuring HomeBanking data services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the HomeBankingContext to the dependency injection container using InMemory provider.
    /// </summary>
    /// <remarks>
    /// This method configures EF Core with the InMemory database provider for development and testing.
    /// Future migrations to SQL Server or PostgreSQL can be made by changing the provider here.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddHomeBankingContext(this IServiceCollection services)
    {
        services.AddDbContext<HomeBankingContext>(options =>
        {
            options.UseInMemoryDatabase("HomeBankingDb");
        });

        return services;
    }
}