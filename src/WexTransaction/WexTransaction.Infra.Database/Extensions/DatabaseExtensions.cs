using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WexTransaction.Infra.Database.Data;

namespace WexTransaction.Infra.Database.Extensions;

/// <summary>
/// Extension methods for database-related operations on the <see cref="WebApplication"/>.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations to the database during application startup.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called in <c>Program.cs</c> during application initialization
    /// to ensure the database schema is up-to-date with the latest migrations.
    ///
    /// If no migrations are pending, this method completes without errors.
    /// </remarks>
    /// <param name="app">The <see cref="WebApplication"/> instance to extend.</param>
    /// <returns>The <see cref="WebApplication"/> instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// var app = builder.Build();
    ///
    /// app.MigrateDatabase();
    ///
    /// app.Run();
    /// </code>
    /// </example>
    public static WebApplication MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger>();

        try
        {
            logger.LogInformation("Applying pending EF Core migrations...");

            var dbContext = scope.ServiceProvider.GetRequiredService<WexTransactionDbContext>();
            dbContext.Database.Migrate();

            logger.LogInformation("Database migrations applied successfully");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Failed to apply migrations: DbContext not configured");
            throw;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to apply migrations: Database error occurred");
            throw;
        }

        return app;
    }
}
