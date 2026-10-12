# RepositoryOptions.cs

**Project:** Budget.Application.Contracts
**Layer:** Contracts / Configuration

## What it does

A strongly-typed configuration class that holds the SQL Server connection string. It's bound from `appsettings.json` and validated at startup.

```json
{
  "ConnectionStrings": {
    "BudgetDatabase": "Server=localhost,1433;Database=BudgetDb;..."
  }
}
```

```csharp
public class RepositoryOptions
{
    [Required]
    public string BudgetDatabaseConnectionString { get; set; }
}
```

## The options pattern

ASP.NET Core's `IOptions<T>` pattern lets you inject typed config anywhere via DI instead of reading raw strings. In `Program.cs`:

```csharp
services.AddOptions<RepositoryOptions>()
    .BindConfiguration("ConnectionStrings")
    .ValidateDataAnnotations()   // enforces [Required]
    .ValidateOnStart();          // fail-fast at startup, not on first use
```

`ValidateOnStart()` is the critical part — if `BudgetDatabaseConnectionString` is missing, the app crashes immediately on startup with a clear error instead of failing later during the first database call.

## Why it's in Application.Contracts

Repository classes need the connection string. If `RepositoryOptions` lived in `Budget.Shared`, then `Budget.Pass` would need a project reference to `Budget.Shared` just to get configuration — creating coupling. Keeping options in `Application.Contracts` lets every project reference it without circular dependencies.
