# FBill.cs

**Project:** Budget.Shared
**Layer:** Domain Model

## What it does

Represents a recurring bill — the template from which payments are generated. Maps to `dbo.Bill`.

| Property | Purpose |
|----------|---------|
| `BillId` | Identity PK |
| `AccountId` | Which account pays this bill |
| `CategoryId` | Spending category (rent, utilities, etc.) |
| `Name` | Human label ("Netflix", "Electric Bill") |
| `ExpectedAmount` | Typical amount (actual may differ) |
| `ScheduleType` | Monthly / Biweekly / Weekly |
| `DayOfMonth` | Used only when `ScheduleType == Monthly` |
| `AnchorDateUTC` | Biweekly/Weekly: first occurrence date to align cadence |
| `IsAutoPay` | If true, payment is auto-submitted on due date |
| `IsActive` | Soft-delete flag |
| `AccountName` | *Joined* from `dbo.Account` — not a real column |
| `CategoryName` | *Joined* from `dbo.Category` — not a real column |

## The join fields pattern

`AccountName` and `CategoryName` have no matching DB columns — they're populated via a SQL JOIN in `RepositoryBill.SelectAll()`. Dapper maps them from the SELECT result into the same C# object. This avoids a separate round-trip to look up display names.

In eMAR the same trick is used: a `FMedication` might carry `PatientName` that's joined in from the patient table.

## AnchorDateUTC

For biweekly bills, the anchor is a known past occurrence. The generator walks from the anchor forward in 14-day steps until it hits the requested window. Example:

```
Anchor = Jan 1, 2024
Window = Feb 1 – Mar 1
Steps: Jan 1 → Jan 15 → Jan 29 → Feb 12 ✓ → Feb 26 ✓ → Mar 11 (stop)
```

Without an anchor, biweekly would have no fixed "phase" — two users with the same bill would generate different days.
