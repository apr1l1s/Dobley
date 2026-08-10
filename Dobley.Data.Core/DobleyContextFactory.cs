using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dobley.Data.Core;

public class DobleyContextFactory : IDesignTimeDbContextFactory<DobleyContext>
{
    public DobleyContext CreateDbContext(string[] args)
    {
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "admin";
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "admin";
        var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "postgres";

        var options = new DbContextOptionsBuilder<DobleyContext>()
            .UseNpgsql($"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}")
            .Options;

        return new DobleyContext(options);
    }
}
