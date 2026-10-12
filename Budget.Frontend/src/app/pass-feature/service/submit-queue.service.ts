import { Injectable } from '@angular/core';
import { Store } from '@ngxs/store';
import { db, QueuedPayment } from '../../core/dexie/budget.database';
import { PassHttpService } from '../../core/http/pass.http.service';
import { UpdatePaymentTempIdAction } from '../store/pass/pass.actions';
import { SubmitPayment } from '../../shared/model/account.model';

/**
 * Drains the offline submit queue when back online.
 * Mirrors eMAR's SubmitQueueService:
 *   - Posts with IsOfflineSync: true so server handles conflicts correctly
 *   - Only deletes queue rows AFTER successful submit (never lose data)
 *   - Dispatches UpdatePaymentTempIdAction with server's TempId→RealId bindings
 */
@Injectable({ providedIn: 'root' })
export class SubmitQueueService {
  constructor(private passHttp: PassHttpService, private store: Store) {
    window.addEventListener('online', () => this.drainQueue());
    // Also drain on startup in case we came back online
    setTimeout(() => this.drainQueue(), 2000);
  }

  async enqueue(payment: SubmitPayment): Promise<void> {
    await db.queueSubmitPayment.add({ payment, queuedAt: new Date().toISOString() });
  }

  async drainQueue(): Promise<void> {
    if (!navigator.onLine) return;
    const queued = await db.queueSubmitPayment.toArray();
    if (queued.length === 0) return;

    const payments = queued.map(q => q.payment);
    this.passHttp.submitPayments(payments, true).subscribe({
      next: async bindings => {
        // Update store with real IDs
        this.store.dispatch(new UpdatePaymentTempIdAction(bindings));
        // Only delete queue rows after successful submit
        const ids = queued.map(q => q.id!).filter(Boolean);
        await db.queueSubmitPayment.bulkDelete(ids);
      },
      error: err => console.error('Queue drain failed, will retry on next online event', err)
    });
  }

  async getQueueLength(): Promise<number> {
    return db.queueSubmitPayment.count();
  }
}
