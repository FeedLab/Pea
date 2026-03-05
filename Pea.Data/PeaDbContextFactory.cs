using Microsoft.EntityFrameworkCore;

namespace Pea.Data;

/// <summary>
/// Factory for creating user-specific database contexts
/// </summary>
public class PeaDbContextFactory
{
    private readonly string serverConnectionString;

    /// <summary>
    /// Creates a factory with a base SQL Server connection string
    /// </summary>
    /// <param name="serverConnectionString">Base connection string (without database name)</param>
    public PeaDbContextFactory(string serverConnectionString)
    {
        this.serverConnectionString = serverConnectionString;
    }

    /// <summary>
    /// Creates a DbContext
    /// Database will be created as Pea on the SQL Server
    /// </summary>
    public PeaDbContext CreateDbContext()
    {
        var databaseName = "Pea";

        // Build connection string with database
        var connectionString = BuildConnectionString(databaseName);

        var context = new PeaDbContext(connectionString);

        // Apply any pending migrations
        context.Database.Migrate();

        return context;
    }

    /// <summary>
    /// Gets the connection string for a specific user
    /// </summary>
    public string GetConnectionString(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        }

        var sanitizedUserId = SanitizeUserId(userId);
        var databaseName = $"Pea_{sanitizedUserId}";
        return BuildConnectionString(databaseName);
    }

    /// <summary>
    /// Gets the database name for a specific user
    /// </summary>
    public string GetDatabaseName(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        }

        var sanitizedUserId = SanitizeUserId(userId);
        return $"Pea_{sanitizedUserId}";
    }

    private string SanitizeUserId(string userId)
    {
        // Remove invalid SQL identifier characters and replace with underscore
        // Keep only alphanumeric and underscore
        var sanitized = new string(userId
            .Where(c => char.IsLetterOrDigit(c) || c == '_')
            .ToArray());

        // Ensure it doesn't start with a number
        if (char.IsDigit(sanitized[0]))
        {
            sanitized = "U_" + sanitized;
        }

        return sanitized;
    }

    private string BuildConnectionString(string databaseName)
    {
        // For SQLite, modify the file path to include the user-specific database
        if (serverConnectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
        {
            // Extract the base path and append user-specific database name
            var dataSourceIndex = serverConnectionString.IndexOf("Data Source=", StringComparison.OrdinalIgnoreCase);
            var dataSourceStart = dataSourceIndex + "Data Source=".Length;
            var semicolonIndex = serverConnectionString.IndexOf(';', dataSourceStart);

            string basePath;
            string remainingConnectionString = "";

            if (semicolonIndex > 0)
            {
                basePath = serverConnectionString.Substring(dataSourceStart, semicolonIndex - dataSourceStart);
                remainingConnectionString = serverConnectionString.Substring(semicolonIndex);
            }
            else
            {
                basePath = serverConnectionString.Substring(dataSourceStart);
            }

            // Get directory and modify filename to include database name
            var directory = Path.GetDirectoryName(basePath);
            var newPath = Path.Combine(directory ?? "", $"{databaseName}.db");

            return $"Data Source={newPath}{remainingConnectionString}";
        }

        // Fallback for other connection string formats
        return $"{serverConnectionString};Database={databaseName}";
    }
}
