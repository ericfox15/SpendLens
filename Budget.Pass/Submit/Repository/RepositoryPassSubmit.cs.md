# RepositoryPassSubmit.cs (and IRepositoryPassSubmit.cs)

**Project:** Budget.Pass
**Layer:** Repository (Data Access)

## What it does

All data access for the submit flow. Four targeted operations:

| Method | Purpose |
|--------|---------|
| `UpdatePayment(payment)` | UPDATE existing payment row (status, amount, note, paidAt) |
| `CreateOrUpdatePayment(payment)` | INSERT for ad-hoc/auto-pay, returns new PaymentId |
| `IsAlreadyRecorded(paymentId)` | Check if a payment is in a terminal state (Paid/Skipped) |
| `InsertPaymentHistory(payment)` | Write audit/conflict row to `dbo.PaymentHistory` |

## IsAlreadyRecorded — offline conflict detection

```sql
SELECT COUNT(1) FROM dbo.Payment
WHERE PaymentId = @Id AND Status IN (1, 2)  -- Paid or Skipped
```

Used by `ServicePassSubmit` during offline sync. If this returns `true`, it means someone already processed this payment while the device was offline. Rather than overwrite, write a history row.

## InsertPaymentHistory — the safety net

```sql
INSERT INTO dbo.PaymentHistory (PaymentId, Amount, Status, Note, ModifiedBy, ModifiedUTC)
VALUES (@PaymentId, @Amount, @Status, @Note, @ModifiedBy, @ModifiedUTC)
```

`dbo.PaymentHistory` preserves conflicting offline submissions. An admin could later review history rows to reconcile discrepancies — useful if two household members submit the same payment from different devices.

## CreateOrUpdatePayment — the MERGE upsert

For auto-pay bills, the service might not know if a payment row already exists (the background job might have created it). Using MERGE lets the submit proceed safely regardless.
