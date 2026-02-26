using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pea.Data;

/// <summary>
/// Design-time factory for Entity Framework migrations
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PeaDbContext>
{
    public PeaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PeaDbContext>();

        // Use a temporary connection string for migrations
        // This will be replaced at runtime with user-specific databases
        optionsBuilder.UseSqlite("Data Source=pea_design.db");

        return new PeaDbContext(optionsBuilder.Options);
    }
}
