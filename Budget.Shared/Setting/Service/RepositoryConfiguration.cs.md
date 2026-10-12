# RepositoryConfiguration.cs

**Project:** Budget.Shared
**Layer:** Service / Configuration Accessor

## What it does

Typed wrapper around `IRepositorySetting` that provides strongly-typed getters for known configuration values. Avoids raw string parsing scattered throughout the codebase.

```csharp
public class RepositoryConfiguration
{
    private readonly IRepositorySetting _setting;

    public bool IsAutoPayShown
        => bool.Parse(_setting.SelectByPath("IsAutoPayShown") ?? "true");

    public decimal GetOverBudgetWarnPercent()
        => decimal.Parse(_setting.SelectByPath("OverBudgetWarnPercent") ?? "80");

    public List<ActionRoute> GetActionRoutes()
        => JsonConvert.DeserializeObject<List<ActionRoute>>(
               _setting.SelectByPath("ActionRoutes") ?? "[]"
           ) ?? new();
}
```

## The action routes setting

`GetActionRoutes()` deserializes a JSON array stored in `dbo.Setting` where `Path = 'ActionRoutes'`. This is the heart of the **configurable action-route pipeline** — the most valuable pattern in the app to understand.

Each `ActionRoute` looks like:

```json
{ "StepType": "SUBMIT_PAYMENT", "Order": 1, "IsEnabled": true }
```

`PaymentService` (Angular) reads these routes from the API via `InitializationHttpService` and uses them to decide what steps to execute when the user clicks "Pay". Steps can be reordered or disabled in the database without a frontend deploy.

## Why not just put these in appsettings.json?

App settings require a restart to change. Database settings can be updated via API and take effect on the next request. This is the same reason eMAR stores facility-specific configurations in the DB rather than config files.
