# PaymentDraftCleanupJob.cs

**Project:** Budget.Api
**Layer:** Background Service

## What it does

A .NET `BackgroundService` that periodically deletes stale Pending payments older than 60 days. Prevents the `dbo.Payment` table from accumulating ghost rows from abandoned pass generations.

## BackgroundService pattern

```csharp
public class PaymentDraftCleanupJob : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await DoCleanup();
            await Task.Delay(TimeSpan.FromSeconds(_interval), stoppingToken);
        }
    }
}
```

`BackgroundService` runs on a background thread for the lifetime of the app. `stoppingToken` is cancelled when the app shuts down, allowing clean teardown.

## IServiceScopeFactory — the scope-per-tick improvement

`BackgroundService` is a singleton (lives for the app lifetime). But `IRepositoryPassGenerate` is scoped (one instance per HTTP request). You can't inject a scoped service directly into a singleton — DI throws.

Solution: inject `IServiceScopeFactory` and create a new scope per tick:

```csharp
using var scope = _factory.CreateScope();
var repo = scope.ServiceProvider.GetRequiredService<IRepositoryPassGenerate>();
await repo.DeleteStalePayments(cutoff);
```

This is the **correct pattern** for all background services that need DB access. The scope is disposed after each tick, releasing the DB connection properly.

## Enabled by config

Only registered if `BackgroundJobs:PaymentCleanup:Enabled = true` in `appsettings.json`. See `BackgroundJobOptions.cs.md`.

## Related: JobScheduler.cs

`PaymentDraftCleanupJob` (this file) uses `BackgroundService`. `JobScheduler.cs` uses **FluentScheduler** for the pass-generation job. Two different scheduling mechanisms — this teaches you both patterns.
