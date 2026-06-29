using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WexTransaction.Infra.Database.Extensions;

/// <summary>
/// Extension methods for database directory initialization on the <see cref="WebApplication"/>.
/// </summary>
public static class DatabaseDirectoryExtensions
{
    /// <summary>
    /// Ensures the database directory exists in the project root with proper permissions (755).
    /// </summary>
    /// <remarks>
    /// This method is intended to be called in <c>Program.cs</c> during application initialization,
    /// BEFORE <c>MigrateDatabase()</c>, to ensure the database directory prerequisites are met
    /// for Docker volume mounts and PostgreSQL data storage.
    ///
    /// On Unix-based systems (Linux, macOS), permissions are set to 755 (rwxr-xr-x).
    /// On Windows, this is a no-op since NTFS handles permissions differently.
    ///
    /// If directory creation fails, a warning is logged but the application continues startup,
    /// allowing PostgreSQL to report specific errors if the directory is truly inaccessible.
    /// </remarks>
    /// <param name="app">The <see cref="WebApplication"/> instance to extend.</param>
    /// <returns>The <see cref="WebApplication"/> instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// var app = builder.Build();
    ///
    /// app.EnsureDatabaseDirectory();  // Create database directory with permissions
    /// app.MigrateDatabase();          // Apply migrations after directory is ready
    ///
    /// app.Run();
    /// </code>
    /// </example>
    public static WebApplication EnsureDatabaseDirectory(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("WexTransaction.Infra.Database.Extensions");

        try
        {
            // Determine the project root directory (where docker-compose.yml and Program.cs are located)
            // Typically: <project-root>/src/WexTransaction/WexTransaction.Api/bin/[Debug|Release]/net10.0/
            // We need to go up several levels to reach the project root
            var currentDirectory = AppContext.BaseDirectory;
            var databaseDirectory = GetDatabaseDirectory(currentDirectory);

            logger.LogInformation("Ensuring database directory exists at {path}", databaseDirectory);

            // Create the directory if it doesn't exist
            Directory.CreateDirectory(databaseDirectory);

            // Set permissions to 755 on Unix-based systems (Linux, macOS)
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    SetUnixPermissions(databaseDirectory);
                    logger.LogInformation("Database directory permissions set to 755");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Warning: Could not set Unix permissions on database directory");
                }
            }
            else
            {
                logger.LogInformation("Database directory created (Windows NTFS permissions used)");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Warning: Could not create/configure database directory");
        }

        return app;
    }

    /// <summary>
    /// Determines the database directory path based on the application's base directory.
    /// Navigates up from the bin directory to find the project root, then returns the database subdirectory.
    /// </summary>
    private static string GetDatabaseDirectory(string baseDirectory)
    {
        // Base directory is typically: <project-root>/src/WexTransaction/WexTransaction.Api/bin/[Debug|Release]/net10.0/
        // We need to navigate up to <project-root>/database/
        var info = new DirectoryInfo(baseDirectory);

        // Navigate up to find the project root (where .git, docker-compose.yml exist)
        // Typically go up 6 levels: net10.0 -> [Debug|Release] -> bin -> WexTransaction.Api -> WexTransaction -> src -> (root)
        while (info.Parent != null)
        {
            // Check if we're at the project root by looking for common markers
            if (FileExists(info.FullName, "docker-compose.yml") ||
                FileExists(info.FullName, ".git") ||
                FileExists(info.FullName, "CLAUDE.md"))
            {
                return Path.Combine(info.FullName, "database");
            }

            info = info.Parent;
        }

        // Fallback: return relative path from current directory
        return Path.Combine(Directory.GetCurrentDirectory(), "database");
    }

    /// <summary>
    /// Checks if a file exists in the given directory.
    /// </summary>
    private static bool FileExists(string directory, string fileName)
    {
        return File.Exists(Path.Combine(directory, fileName)) ||
               Directory.Exists(Path.Combine(directory, fileName));
    }

    /// <summary>
    /// Sets Unix file permissions to 755 (rwxr-xr-x) on the directory.
    /// </summary>
    /// <remarks>
    /// This method uses the <see cref="FileInfo.UnixFileMode"/> property (available in .NET 5+)
    /// to set permissions. On Windows, this method will throw an exception which should be caught.
    /// </remarks>
    private static void SetUnixPermissions(string directoryPath)
    {
        var info = new DirectoryInfo(directoryPath);

        // 755 = rwxr-xr-x (owner: rwx, group: r-x, other: r-x)
        // In UnixFileMode: User (Read, Write, Execute) | Group (Read, Execute) | Others (Read, Execute)
        const UnixFileMode permissions = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                                        UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                                        UnixFileMode.OtherRead | UnixFileMode.OtherExecute;

        info.UnixFileMode = permissions;
    }
}
