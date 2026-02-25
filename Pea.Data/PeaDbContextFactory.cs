using Microsoft.EntityFrameworkCore;

namespace Pea.Data;

/// <summary>
/// Factory for creating user-specific database contexts
/// </summary>
public class PeaDbContextFactory
{
    private readonly string _serverConnectionString;

    /// <summary>
    /// Creates a factory with a base SQL Server connection string
    /// </summary>
    /// <param name="serverConnectionString">Base connection string (without database name)</param>
    public PeaDbContextFactory(string serverConnectionString)
    {
        _serverConnectionString = serverConnectionString;
    }

    /// <summary>
    /// Creates a DbContext for a specific user
    /// Database will be created as Pea_{sanitizedUserId} on the SQL Server
    /// </summary>
    public PeaDbContext CreateDbContext(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        }

        // Sanitize userId to create valid database name
        var sanitizedUserId = SanitizeUserId(userId);
        var databaseName = $"Pea_{sanitizedUserId}";

        // Build connection string with user-specific database
        var connectionString = BuildConnectionString(databaseName);

        var context = new PeaDbContext(connectionString);

        // Ensure database is created
        context.Database.EnsureCreated();

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
        // Check if the base connection string already has a database
        if (_serverConnectionString.Contains("Database=", StringComparison.OrdinalIgnoreCase) ||
            _serverConnectionString.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase))
        {
            // Replace existing database name
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(_serverConnectionString)
            {
                InitialCatalog = databaseName
            };
            return builder.ConnectionString;
        }
        else
        {
            // Append database name to connection string
            return $"{_serverConnectionString};Database={databaseName}";
        }
    }
}
