import Dexie, { Table } from 'dexie';
import { FAccount, FBill, FPayment, SubmitPayment } from '../../shared/model/account.model';

/**
 * IndexedDB database for offline support. Mirrors eMAR's BudgetDatabase extends Dexie.
 * Tables: cached accounts/bills/payments (last generated pass) + offline submit queue.
 * IMPORTANT: Never edit an old version() block — always add a new one for schema changes.
 */
export class BudgetDatabase extends Dexie {
  accounts!: Table<FAccount, number>;
  bills!: Table<FBill, number>;
  payments!: Table<FPayment, number>;
  queueSubmitPayment!: Table<QueuedPayment, number>;

  constructor() {
    super('BudgetHelperDB');
    this.version(1).stores({
      accounts:            'AccountId, Name',
      bills:               'BillId, AccountId, CategoryId',
      payments:            'PaymentId, BillId, AccountId, Status, DueAtUTC',
      queueSubmitPayment:  '++id, PaymentId'
    });
  }
}

export interface QueuedPayment {
  id?: number;
  payment: SubmitPayment;
  queuedAt: string;
}

export const db = new BudgetDatabase();
