using CollectManagement.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CollectManagement.Tests.Integration;

/// <summary>
/// Helper that creates a fresh in-memory ApplicationDbContext for each test.
/// </summary>
public static class InMemoryDbHelper
{
    public static ApplicationDbContext CreateContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
