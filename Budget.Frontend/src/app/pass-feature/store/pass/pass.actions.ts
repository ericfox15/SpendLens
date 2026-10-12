import { FPayment, PaymentBindingId, EPaymentStatus } from '../../../shared/model/account.model';

/**
 * NGXS actions follow the class-per-action pattern from eMAR.
 * The store is the single source of truth for the current pass window.
 */

export class GeneratePassAction {
  static readonly type = '[Pass] Generate';
  constructor(public startUTC: string, public endUTC: string) {}
}

export class PayAction {
  static readonly type = '[Pass] Pay';
  constructor(public paymentId: number, public amount: number, public note: string | null) {}
}

export class SkipAction {
  static readonly type = '[Pass] Skip';
  constructor(public paymentId: number, public note: string) {}
}

export class ResetAction {
  static readonly type = '[Pass] Reset';
  constructor(public paymentId: number) {}
}

export class GenerateAdHocPaymentAction {
  static readonly type = '[Pass] Generate AdHoc';
  constructor(public payment: Partial<FPayment>) {}
}

export class UpdatePaymentTempIdAction {
  static readonly type = '[Pass] Update TempId';
  constructor(public bindings: PaymentBindingId[]) {}
}

export class ClearPassAllAction {
  static readonly type = '[Pass] Clear All';
}
