# budget.database.ts

**Project:** Budget.Frontend
**Layer:** Core / Offline Storage (Dexie / IndexedDB)

## What it does

Defines the client-side IndexedDB schema using Dexie.js. This is the offline cache and submit queue for the app.

```typescript
export class BudgetDatabase extends Dexie {
  accounts!: Table<FAccount>;
  bills!: Table<FBill>;
  payments!: Table<FPayment>;
  queueSubmitPayment!: Table<SubmitPayment>;

  constructor() {
    super('BudgetDatabase');
    this.version(1).stores({
      accounts:            '++AccountId, Name',
      bills:               '++BillId, AccountId, CategoryId',
      payments:            '++PaymentId, BillId, DueAtUTC, Status',
      queueSubmitPayment:  '++id'
    });
  }
}
```

## Why Dexie?

IndexedDB's native API is verbose and callback-based. Dexie wraps it with:
- Promise-based queries
- Simple `table.add()`, `table.put()`, `table.where().equals().toArray()` API
- TypeScript generics for type-safe table access

## The submit queue table

`queueSubmitPayment` is the **offline queue**. When the user is offline and pays a bill:

1. `PaymentService` can't reach the API.
2. The payment is added to `queueSubmitPayment` instead.
3. `SubmitQueueService` listens for the browser's `online` event.
4. When connectivity returns, it drains the queue — sends all queued payments with `IsOfflineSync: true`.

## eMAR parallel

eMAR uses the same Dexie pattern for its medication pass offline cache. Nurses can administer medications without internet; the app queues them locally and syncs when the device reconnects.

## Cache vs queue

- `accounts`, `bills`, `payments` — read cache (mirrors server data for offline display)
- `queueSubmitPayment` — write queue (pending submissions that haven't reached the server)
