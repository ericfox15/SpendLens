# ServicePassSubmit.cs (and IServicePassSubmit.cs)

**Project:** Budget.Pass
**Layer:** Service (Core Business Logic)

## What it does

Handles the batch payment submission: takes a list of user-submitted payments (with temp IDs), persists them to the DB, and returns the real `PaymentId` mappings.

## SubmitPaymentsAsync — the flow

```
For each payment in request.Payments:
  1. Open a TransactionScope (one per payment)
  2. If TempId < 0 (new ad-hoc): INSERT → get RealId
  3. If TempId > 0 (existing): UPDATE existing row
  4. If IsOfflineSync AND payment already in a terminal state → write PaymentHistory row
  5. Commit TransactionScope
  6. Return PaymentBindingId { TempId, RealId }
```

## TransactionScope — one per payment

Each payment is wrapped in its own `TransactionScope` rather than one scope for the entire batch. This means:
- If payment 3 fails, payments 1 and 2 are already committed — not rolled back.
- This is intentional: partial success is better than losing all work on a batch error.

In eMAR, the same pattern exists because each medication administration is an independent clinical event.

```csharp
using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
// ... do work ...
scope.Complete();
```

## Temp negative IDs

The Angular NGXS store assigns negative IDs (`-1`, `-2`, ...) to locally-created payments. When submitted, the server returns:

```json
[{ "TempId": -1, "RealId": 1042 }, { "TempId": -2, "RealId": 1043 }]
```

`UpdatePaymentTempIdAction` in the NGXS store replaces each temp ID with its real ID.

## IsOfflineSync conflict handling

If `IsOfflineSync = true` (the request came from the offline queue) and the payment was already updated by another session, the service writes a `PaymentHistory` row instead of overwriting. This preserves both the current state and the offline record for reconciliation.

## eMAR parallel

eMAR's `ServiceMedPassSubmit` uses the exact same pattern: temp IDs, one TransactionScope per administration, offline sync conflict → history row.
