# ServiceBill.cs (and IServiceBill.cs)

**Project:** Budget.Shared
**Layer:** Service

## What it does

Business logic layer for bills. Mostly thin delegation to `IRepositoryBill`, similar to `ServiceAccount`.

## Key business rule: AnchorDateUTC default

When creating a biweekly or weekly bill without a specified anchor, the service sets `AnchorDateUTC = DateTimeOffset.UtcNow` so the first occurrence aligns to today. Without this default, the pass generator would have `null` as the anchor and throw.

```csharp
public async Task<int> Create(FBill bill)
{
    if (bill.ScheduleType != EScheduleType.Monthly && bill.AnchorDateUTC == default)
        bill.AnchorDateUTC = DateTimeOffset.UtcNow;
    return await _repo.Insert(bill);
}
```

## Why bills matter

Bills are templates. They have no financial impact on their own — only when the pass generator expands them into `FPayment` rows do they become actionable. If you delete a bill (deactivate it), existing payments remain untouched; only future generation skips it.

Think of it like eMAR: a medication order (bill) generates scheduled administration records (payments). Stopping the order doesn't undo past administrations.
