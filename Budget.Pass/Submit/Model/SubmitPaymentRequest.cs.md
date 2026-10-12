# SubmitPaymentRequest.cs

**Project:** Budget.Pass
**Layer:** Model (API Contract)

## What it does

Request body for `POST /api/pass/submit/payment`.

```csharp
public class SubmitPaymentRequest
{
    public List<SubmitPayment> Payments { get; set; }
    public bool IsOfflineSync { get; set; }
}

public class SubmitPayment
{
    public int TempId { get; set; }      // negative = new ad-hoc, positive = existing
    public int? BillId { get; set; }
    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public DateTimeOffset DueAtUTC { get; set; }
    public DateTimeOffset? PaidAtUTC { get; set; }
    public decimal Amount { get; set; }
    public EPaymentStatus Status { get; set; }
    public string Note { get; set; }
}
```

## `PaymentBindingId` — the response

```csharp
public class PaymentBindingId
{
    public int TempId { get; set; }
    public int RealId { get; set; }
}
```

After submission, the Angular store receives this array and uses `UpdatePaymentTempIdAction` to replace each temp ID in state with its real DB-assigned ID.

## IsOfflineSync flag

Set to `true` by `SubmitQueueService` when draining the offline queue. Tells the server to check for conflicts and write history rows instead of overwriting.

## Why batch rather than per-payment?

A batch submit minimizes round trips. The user might have paid 5 bills in one session — sending one request instead of five reduces latency and allows the server to optimize (e.g., bulk INSERT in one transaction).
