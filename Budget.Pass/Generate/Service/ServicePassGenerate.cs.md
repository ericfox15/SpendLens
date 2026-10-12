# ServicePassGenerate.cs (and IServicePassGenerate.cs)

**Project:** Budget.Pass
**Layer:** Service (Core Business Logic)

## What it does

The most important service in the app. Turns a list of active bills into payment records for a requested time window.

## GeneratePassAsync — the main flow

```
1. Load all active bills from DB
2. Load existing payments for the window (to avoid duplicates)
3. Call ExpandSchedules(bills, start, end) → List<(BillId, DueAt)>
4. For each expanded date:
   - If a payment already exists for (BillId, DueAt) → reuse it (keep Paid/Skipped status)
   - If not → INSERT a new Pending payment
5. Split payments into Standard / Extra / Optional buckets
6. Return PassGenerateResponse
```

## ExpandSchedules — the key algorithm

Private method that calculates every due date within [start, end) for each bill:

```csharp
Monthly:
  dueDate = new DateTimeOffset(year, month, Math.Min(bill.DayOfMonth, daysInMonth), 0,0,0, TimeSpan.Zero)

Biweekly:
  start from anchor, step +14 days until inside window

Weekly:
  start from anchor, step +7 days until inside window
```

### Month-end clamping

```csharp
var daysInMonth = DateTime.DaysInMonth(year, month);
var day = Math.Min(bill.DayOfMonth, daysInMonth);
```

A bill set to day 31 gets clamped to the last day of the month. Tested in `ServicePassGenerateTests` and `ActionRouteTests`.

### Biweekly alignment

```csharp
var diff     = (start - anchor).TotalDays;
var stepsBack = (int)Math.Floor(diff / 14.0);
var current  = anchor.AddDays(stepsBack * 14);
while (current < start) current = current.AddDays(14);
```

This "walks back" to find the most recent biweekly occurrence before the window, then steps forward. Without this alignment, the first date in the window would always be `anchor + n*14` which could skip days inside the window.

## GenerateNextPeriodAsync — for background jobs

Called by `JobScheduler` every 15 minutes. Generates payments for the next 30 days automatically so the user always has a ready pass when they open the app.

## eMAR parallel

`ServicePassGenerate` mirrors eMAR's `ServiceMedPassGenerate` — it expands medication schedules (QD, BID, TID, Q4H, PRN) into administration records for a shift window. The scheduling math is essentially the same problem.
