# BackgroundJobOptions.cs

**Project:** Budget.Application.Contracts
**Layer:** Contracts / Configuration

## What it does

Configuration for background jobs. Currently controls the `PaymentCleanup` job that purges stale pending payments.

```json
{
  "BackgroundJobs": {
    "PaymentCleanup": {
      "Enabled": true,
      "IntervalSeconds": 3600
    }
  }
}
```

## Design: feature flags for background jobs

Wrapping each job in a `{ Enabled, IntervalSeconds }` object (rather than a simple boolean) lets you:
1. Disable a job without redeploying (restart + config change only).
2. Tune the interval per environment (e.g., run hourly in production, every 60 seconds in dev for testing).

In `Program.cs`, the startup code reads this config and only registers the `BackgroundService` if `Enabled == true`:

```csharp
if (app.Configuration.GetSection("BackgroundJobs:PaymentCleanup").Get<JobEntry>()?.Enabled == true)
    services.AddHostedService<PaymentDraftCleanupJob>();
```

## Related files

- `PaymentDraftCleanupJob.cs` — the `BackgroundService` that actually runs
- `JobScheduler.cs` — FluentScheduler-based pass generation job (runs every 15 min)
