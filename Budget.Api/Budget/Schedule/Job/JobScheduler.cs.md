# JobScheduler.cs

**Project:** Budget.Api
**Layer:** Background Job (FluentScheduler)

## What it does

Registers a recurring FluentScheduler job that automatically generates payments for the next 30 days every 15 minutes. Users always have a pre-populated pass ready when they open the app.

## FluentScheduler vs BackgroundService

The app uses **two** background job mechanisms to demonstrate both patterns:

| Mechanism | Class | Trigger |
|-----------|-------|---------|
| FluentScheduler | `JobScheduler` | Every 15 minutes (cron-like) |
| BackgroundService | `PaymentDraftCleanupJob` | Every N seconds (polling loop) |

FluentScheduler is better for calendar-based schedules ("every day at 2am"). BackgroundService is better for continuous polling loops.

## Explicit constructor (not primary constructor)

The class uses an **explicit constructor with a stored field** rather than C# 12 primary constructors:

```csharp
// WRONG for this case (causes CS8862 compile error):
public class JobScheduler(IServiceProvider sp) : Registry { ... }

// CORRECT:
public class JobScheduler : Registry
{
    private readonly IServiceProvider _serviceProvider;
    public JobScheduler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        // schedule setup here
    }
}
```

Primary constructor parameters can't be used in initializer expressions — a limitation when the base class `Registry` requires schedule setup in the constructor body.

## Scope-per-tick pattern

Same as `PaymentDraftCleanupJob`:
```csharp
Schedule(() =>
{
    using var scope = _serviceProvider.CreateScope();
    var svc = scope.ServiceProvider.GetRequiredService<IServicePassGenerate>();
    svc.GenerateNextPeriodAsync().GetAwaiter().GetResult();
}).ToRunEvery(15).Minutes();
```

`.GetAwaiter().GetResult()` is used because FluentScheduler jobs are synchronous. This is acceptable here because the job runs on a dedicated background thread, not the ASP.NET Core thread pool.

## Registration in Program.cs

```csharp
JobManager.Initialize(new JobScheduler(app.Services));
```
