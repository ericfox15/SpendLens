import { Injectable } from '@angular/core';
import { Store } from '@ngxs/store';
import { PassHttpService } from '../../core/http/pass.http.service';
import { SubmitQueueService } from './submit-queue.service';
import { PayAction, SkipAction, ResetAction, UpdatePaymentTempIdAction } from '../store/pass/pass.actions';
import { SubmitPayment, FPayment, EPaymentStatus } from '../../shared/model/account.model';
import { ActionRoute, StepPayload, isValidActionRoute } from '../path/action-route.model';

/**
 * Orchestrates payment actions by running the configurable step pipeline.
 * Each step is a simple function that either mutates the payload and continues,
 * or short-circuits (user cancelled, check failed) by setting payload.cancelled = true.
 *
 * This is the heart of the eMAR pattern — adding a new modal step just requires
 * adding a new StepType and updating the Setting table, no code changes needed.
 */
@Injectable({ providedIn: 'root' })
export class PaymentService {
  private actionRoutes: Record<string, ActionRoute> = {};

  constructor(
    private store: Store,
    private passHttp: PassHttpService,
    private submitQueue: SubmitQueueService
  ) {}

  setActionRoutes(routes: Record<string, ActionRoute>) {
    this.actionRoutes = routes;
  }

  async runAction(routeName: string, payment: FPayment, overBudget = false): Promise<void> {
    const route = this.actionRoutes[routeName];
    if (!route || !isValidActionRoute(route)) {
      console.warn(`ActionRoute '${routeName}' missing or invalid — running simple submit`);
      await this.simpleSubmit(routeName, payment);
      return;
    }

    const payload: StepPayload = { paymentId: payment.PaymentId };

    for (const step of route.steps) {
      if (payload.cancelled) return;

      switch (step) {
        case 'CHECK_OVER_BUDGET':
          if (!overBudget) { /* proceed */ } else {
            payload.cancelled = !confirm('This payment exceeds the category budget. Continue?');
          }
          break;

        case 'CHECK_LARGE_PAYMENT':
          if ((payload.amount ?? payment.Amount ?? 0) > 500) {
            payload.cancelled = !confirm(`Large payment of $${payload.amount ?? payment.Amount}. Confirm?`);
          }
          break;

        case 'QUEUE_ADD_CONFIRMATION':
          // Already handled by CHECK_LARGE_PAYMENT — proceed
          break;

        case 'GET_NOTE': {
          const note = prompt('Add a note (optional):');
          payload.note = note ?? null;
          break;
        }

        case 'GET_SKIP_REASON': {
          const reason = prompt('Reason for skipping (required):');
          if (!reason) { payload.cancelled = true; break; }
          payload.note = reason;
          break;
        }

        case 'GET_AMOUNT': {
          const amountStr = prompt(`Amount paid (expected: $${payment.ExpectedAmount ?? payment.Amount ?? 0}):`, String(payment.ExpectedAmount ?? payment.Amount ?? 0));
          if (!amountStr) { payload.cancelled = true; break; }
          payload.amount = parseFloat(amountStr);
          break;
        }

        case 'SUBMIT_ONLINE':
        case 'SUBMIT_BOTH':
          await this.submitPayments(routeName, payment, payload, false);
          if (step === 'SUBMIT_BOTH' && !navigator.onLine) {
            await this.submitQueue.enqueue(this.buildSubmitPayment(routeName, payment, payload));
          }
          break;

        case 'SUBMIT_OFFLINE':
          await this.submitQueue.enqueue(this.buildSubmitPayment(routeName, payment, payload));
          break;

        case 'NOTIFICATION_SUCCESS':
          console.log(`✓ ${routeName} completed for payment ${payment.PaymentId}`);
          break;

        case 'KILL':
          payload.cancelled = true;
          break;
      }
    }
  }

  private async submitPayments(routeName: string, payment: FPayment, payload: StepPayload, isOfflineSync: boolean) {
    const status = routeName === 'PAY' ? EPaymentStatus.Paid :
                   routeName === 'SKIP' ? EPaymentStatus.Skipped : EPaymentStatus.Pending;

    // Update local state immediately (optimistic)
    if (routeName === 'PAY') {
      this.store.dispatch(new PayAction(payment.PaymentId, payload.amount ?? payment.Amount ?? payment.ExpectedAmount ?? 0, payload.note ?? null));
    } else if (routeName === 'SKIP') {
      this.store.dispatch(new SkipAction(payment.PaymentId, payload.note ?? 'Skipped'));
    } else if (routeName === 'RESET') {
      this.store.dispatch(new ResetAction(payment.PaymentId));
    }

    if (navigator.onLine) {
      const submitPayment = this.buildSubmitPayment(routeName, payment, payload);
      this.passHttp.submitPayments([submitPayment], isOfflineSync).subscribe({
        next: bindings => this.store.dispatch(new UpdatePaymentTempIdAction(bindings)),
        error: async err => {
          // If online submit fails, push to offline queue
          console.warn('Submit failed, queuing for later', err);
          await this.submitQueue.enqueue(this.buildSubmitPayment(routeName, payment, payload));
        }
      });
    } else {
      await this.submitQueue.enqueue(this.buildSubmitPayment(routeName, payment, payload));
    }
  }

  private async simpleSubmit(routeName: string, payment: FPayment) {
    const payload: StepPayload = { paymentId: payment.PaymentId };
    await this.submitPayments(routeName, payment, payload, false);
  }

  private buildSubmitPayment(routeName: string, payment: FPayment, payload: StepPayload): SubmitPayment {
    const status = routeName === 'PAY' ? EPaymentStatus.Paid :
                   routeName === 'SKIP' ? EPaymentStatus.Skipped : EPaymentStatus.Pending;
    return {
      Standard: {
        PaymentId: payment.PaymentId,
        BillId: payment.BillId ?? 0,
        AccountId: payment.AccountId,
        CategoryId: payment.CategoryId,
        DueAtUTC: payment.DueAtUTC,
        PaidAtUTC: status === EPaymentStatus.Paid ? new Date().toISOString() : null,
        Amount: payload.amount ?? payment.Amount,
        Status: status,
        Note: payload.note ?? null,
        ModifiedBy: 0  // replaced with actual userId from JWT claims on server
      }
    };
  }
}
