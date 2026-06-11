using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MovieReview.Api.Data;

namespace MovieReview.Tests.Repositories;

/// <summary>
/// Hosts a fresh SQLite in-memory database per test class. Unlike the EF InMemory provider,
/// SQLite enforces unique indexes, check constraints and foreign keys — the things we test.
/// </summary>
public abstract class SqliteRepositoryTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly AppDbContext Context;

    protected SqliteRepositoryTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
