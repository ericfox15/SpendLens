# FPayment.cs

**Project:** Budget.Shared
**Layer:** Domain Model

## What it does

A single payment event — either generated from a bill schedule or entered ad-hoc. Maps to `dbo.Payment`.

| Property | Notes |
|----------|-------|
| `PaymentId` | Identity PK. Negative values are **temp IDs** used client-side before submission. |
| `BillId` | FK to `dbo.Bill`. **Nullable** — null means ad-hoc payment not tied to any bill. |
| `AccountId` | FK to `dbo.Account` |
| `CategoryId` | FK to `dbo.Category` |
| `DueAtUTC` | When the payment is due |
| `PaidAtUTC` | When it was actually paid (null if still pending/skipped) |
| `Amount` | Actual amount paid (may differ from `Bill.ExpectedAmount`) |
| `Status` | `EPaymentStatus`: Pending / Paid / Skipped |
| `Note` | Optional text ("paid early", "split with partner") |
| `ModifiedBy` | Username of last modifier (from JWT claim) |
| `ModifiedUTC` | Last modification timestamp |
| `BillName`, `AccountName`, `CategoryName` | Joined fields, not DB columns |

## Temp negative IDs

The Angular frontend (NGXS state) assigns temp IDs (`-1`, `-2`, ...) to payments the user creates locally before the submit batch completes. After `POST /api/pass/submit/payment`, the server returns `PaymentBindingId[]` — an array of `{ TempId, RealId }` mappings. The store then replaces each temp ID with its real server ID.

This is the exact same offline-first pattern eMAR uses for medication passes.

## UNIQUE constraint

`dbo.Payment` has a unique index on `(BillId, DueAtUTC)` — you can only have one payment per bill per due date. The generator checks for existing payments before inserting, and the database enforces the rule as a safety net.
