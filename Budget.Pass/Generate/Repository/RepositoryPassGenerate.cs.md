# RepositoryPassGenerate.cs (and IRepositoryPassGenerate.cs)

**Project:** Budget.Pass
**Layer:** Repository (Data Access)

## What it does

Provides all data access needed by `ServicePassGenerate`. Intentionally scoped to read operations and the initial payment insertion — it does not handle payment updates (that's `RepositoryPassSubmit`).

| Method | Purpose |
|--------|---------|
| `SelectActiveBills()` | All active bills with account + category names joined |
| `SelectExistingPayments(start, end)` | Payments in the window — used to avoid duplicate generation |
| `InsertPayment(payment)` | Insert a new Pending payment, returns new PaymentId |
| `SelectAdherenceFlags()` | Summary stats for dashboard widgets |
| `SelectOverdueCount()` | Count of payments past due and still Pending |

## SelectExistingPayments — the deduplication query

```sql
SELECT * FROM dbo.Payment
WHERE DueAtUTC >= @Start AND DueAtUTC < @End
```

`ServicePassGenerate` puts these in a `HashSet<(int BillId, DateTimeOffset DueAt)>` for O(1) lookup. For each expanded date, if `(billId, dueAt)` is already in the set, it skips insertion and returns the existing payment row unchanged.

This is critical: if a user already marked a payment as Paid and the app regenerates the window, the Paid status must be preserved. Regeneration is **idempotent**.

## InsertPayment — duplicate safety net

Even though the service checks first, the database has a `UNIQUE` constraint on `(BillId, DueAtUTC)`. If a race condition causes two concurrent generate calls to try inserting the same payment, the DB constraint catches it. The INSERT uses `TRY...CATCH` to swallow the duplicate key error gracefully.
