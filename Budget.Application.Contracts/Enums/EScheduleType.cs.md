# EScheduleType.cs

**Project:** Budget.Application.Contracts
**Layer:** Contracts

## What it does

Defines how often a bill repeats:

```
Monthly   = 1   → once per month on a specific day (uses Bill.DayOfMonth)
Biweekly  = 2   → every 14 days from an anchor date (uses Bill.AnchorDateUTC)
Weekly    = 3   → every 7 days from an anchor date
```

## How it drives the pass generator

`ServicePassGenerate.ExpandSchedules()` switches on this enum to decide how to calculate due dates within a requested window:

| Value | Logic |
|-------|-------|
| Monthly | `new DateTimeOffset(year, month, Math.Min(DayOfMonth, daysInMonth), ...)` |
| Biweekly | Walk anchor forward by 14-day steps until inside [Start, End) |
| Weekly | Same as biweekly but 7-day steps |

The `Math.Min(DayOfMonth, daysInMonth)` clamping is important — a bill set to day 31 gets moved to the last day of February (28 or 29), not skipped.

## Angular equivalent

The bill form has a `<select>` bound to `EScheduleType`, and `getScheduleName()` in `BillComponent` maps values back to labels. The TypeScript enum must stay numerically aligned with this C# enum.
