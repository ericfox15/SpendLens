# FRepositoryBase.cs

**Project:** Budget.Shared
**Layer:** Data Access (Repository Base)

## What it does

Abstract base class for all repository classes in the app. Its only job is to provide a `GetConnection()` helper that creates a new `SqlConnection` using the connection string from `RepositoryOptions`.

```csharp
public abstract class FRepositoryBase
{
    private readonly string _connectionString;

    protected FRepositoryBase(IOptions<RepositoryOptions> options)
        => _connectionString = options.Value.BudgetDatabaseConnectionString;

    protected SqlConnection GetConnection()
        => new SqlConnection(_connectionString);
}
```

## The F* naming convention

In eMAR, `F` prefix means **domain model** — something that represents a first-class entity in the system. `FRepositoryBase` uses the prefix to signal it's a framework-level base class in the eMAR style, not a plain utility.

## Why create a new connection per call?

Dapper (and ADO.NET in general) work best with **short-lived connections**. `GetConnection()` returns an unopened connection; Dapper opens it internally when you call `.Query()` or `.Execute()`, then closes it immediately after. This is deliberate — it plays nicely with connection pooling in `Microsoft.Data.SqlClient`.

**Never** store a long-lived `SqlConnection` as a field on the repository. That would cause threading issues and connection leaks.

## Dapper pattern reminder

```csharp
// Typical repository method using this base:
public async Task<FAccount> SelectById(int id)
{
    using var conn = GetConnection();
    return await conn.QuerySingleOrDefaultAsync<FAccount>(
        "SELECT * FROM dbo.Account WHERE AccountId = @Id AND IsActive = 1",
        new { Id = id }
    );
}
```

The `using` ensures the connection is disposed (returned to the pool) after the query completes.
