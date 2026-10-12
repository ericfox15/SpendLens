# Program.cs (Budget.Database)

**Project:** Budget.Database
**Layer:** Database Migration Runner

## What it does

A standalone console application that runs DbUp migrations against SQL Server. Run this once to set up the database schema before starting the API.

## DbUp concepts

**DbUp** is a library that:
1. Tracks which SQL scripts have already been applied (in `dbo.SchemaVersions`).
2. Runs only new scripts in order.
3. Is idempotent — running it again on an already-migrated DB does nothing.

```csharp
var upgrader = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(),
        s => s.Contains("DeploymentScript"))
    .JournalToSqlTable("dbo", "SchemaVersions")
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();
if (!result.Successful) Environment.Exit(1);
```

## Script types

| Folder | DbUp table | Re-runs? | Use for |
|--------|-----------|---------|---------|
| `DeploymentScript/` | `dbo.SchemaVersions` | No | Tables, FK constraints, seed data |
| `RunAlways/` | `dbo.SchemaVersionsRunAlways` | Yes (every time) | Views, stored procedures (CREATE OR ALTER) |

**DeploymentScript** scripts are tracked: once run, never re-run. Use for anything that changes state (CREATE TABLE, INSERT seed data).

**RunAlways** scripts run on every deployment: use for `CREATE OR ALTER VIEW` and `CREATE OR ALTER PROCEDURE` — these are idempotent by design and should always reflect the latest version.

## Connection string resolution

```csharp
// Priority: CLI arg > env var > appsettings.json
string connStr = args.FirstOrDefault()
    ?? Environment.GetEnvironmentVariable("BudgetDatabase")
    ?? config.GetConnectionString("BudgetDatabase");
```

This lets you run it via Docker with an env var, locally with appsettings, or in CI with a CLI arg.
