# submit-queue.service.ts

**Project:** Budget.Frontend
**Layer:** Pass Feature / Service

## What it does

Manages the offline submit queue. When the device regains internet connectivity, this service drains all queued payments to the server.

## How it works

```typescript
@Injectable({ providedIn: 'root' })
export class SubmitQueueService {
  constructor(private db: BudgetDatabase, private passHttp: PassHttpService) {
    window.addEventListener('online', () => this.drain());
  }

  async drain(): Promise<void> {
    const queued = await this.db.queueSubmitPayment.toArray();
    if (!queued.length) return;

    this.passHttp
      .submitPayments(queued, /* isOfflineSync = */ true)
      .subscribe({
        next: () => this.db.queueSubmitPayment.clear(),
        error: () => { /* leave in queue, try again next time */ }
      });
  }
}
```

## IsOfflineSync: true

Queued submissions are sent with `IsOfflineSync: true`. The server uses this flag to trigger conflict detection — if a payment was already updated by another session, the server writes a `PaymentHistory` row instead of overwriting.

## Delete ONLY after success

Critically, the queue is only cleared **after** a successful API response. If the network fails mid-drain, the queue retains all unprocessed payments and will retry on the next `online` event.

If you cleared the queue before the API call, a network failure would silently lose the user's work.

## Initialization

`SubmitQueueService` is injected in `AppComponent` (or `app-module.ts`) to ensure it's instantiated at startup and registers the `online` event listener.

## eMAR parallel

eMAR's offline queue uses the same pattern. Nurses administer medications while offline; the app queues them, and the sync service sends them when the tablet reconnects to the facility network.
