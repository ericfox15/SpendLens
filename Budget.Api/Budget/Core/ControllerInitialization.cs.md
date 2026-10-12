# ControllerInitialization.cs

**Project:** Budget.Api
**Layer:** Controller (HTTP Entry Point)

## What it does

Returns app initialization data — settings the Angular frontend needs before it can function fully.

| Method | Route | Description |
|--------|-------|-------------|
| GET | `api/core/initialization/settings` | Returns `InitializationSettings` |

## InitializationSettings

```csharp
public class InitializationSettings
{
    public bool IsAutoPayShown { get; set; }
    public decimal OverBudgetWarnPercent { get; set; }
    public List<ActionRoute> ActionRoutes { get; set; }
}
```

## Why load settings at startup?

The Angular app calls this endpoint during `APP_INITIALIZER` (before the first page renders) to load:

1. **`IsAutoPayShown`** — whether the "Optional" (auto-pay) section appears in the pass UI.
2. **`OverBudgetWarnPercent`** — threshold for showing the yellow warning on the report.
3. **`ActionRoutes`** — the configurable step pipeline for `PaymentService.runAction()`.

Loading these at startup avoids per-request DB hits. The settings are cached in `InitializationHttpService` and used throughout the Angular session.

## The action routes are the key part

`ActionRoutes` is the serialized JSON from `dbo.Setting` where `Path = 'ActionRoutes'`. It defines what steps the Angular payment flow executes in sequence. See `action-route.model.ts.md` and `payment.service.ts.md` for how Angular uses these.

## eMAR parallel

eMAR has an initialization endpoint that loads facility settings, medication schedules, and workflow configurations before the app renders. This is the same concept — load configuration once, use it throughout the session.
