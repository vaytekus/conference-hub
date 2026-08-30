using ConferenceHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace ConferenceHub.IntegrationTests.Infrastructure;

public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;
        return new AppDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
