using Microsoft.AspNetCore.Builder;

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
        throw new NotImplementedException("Implementation will be added in Task 2.");
    }
}
