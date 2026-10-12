# FSetting.cs

**Project:** Budget.Shared
**Layer:** Domain Model

## What it does

Simple key-value store backed by `dbo.Setting`. Used for app-wide configuration that needs to be changeable without redeploying.

```
Path  → key   (e.g., "ActionRoutes", "OverBudgetWarnPercent")
Value → value (stored as string, parsed by callers)
```

## Why a database settings table?

Some settings benefit from being adjustable at runtime:
- Action routes (the configurable step pipeline) are stored here as JSON.
- Over-budget warning thresholds.
- Feature flags (e.g., `IsAutoPayShown`).

Compared to `appsettings.json`, `dbo.Setting` values can be changed via API calls without a restart.

## RepositorySetting uses MERGE

The upsert uses SQL MERGE so you can call "save setting" idempotently — it inserts on first use, updates on subsequent calls. No "does it exist?" check needed in C#.

## RepositoryConfiguration

`RepositoryConfiguration.cs` wraps `RepositorySetting` and provides **typed getters** that parse the raw strings:

```csharp
public bool IsAutoPayShown => bool.Parse(Get("IsAutoPayShown") ?? "true");
public decimal GetOverBudgetWarnPercent() => decimal.Parse(Get("OverBudgetWarnPercent") ?? "80");
public List<ActionRoute> GetActionRoutes() => JsonConvert.DeserializeObject<List<ActionRoute>>(Get("ActionRoutes") ?? "[]");
```

This is the same pattern as eMAR's `RepositoryConfiguration`.
