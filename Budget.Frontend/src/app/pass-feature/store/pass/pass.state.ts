import { State, Action, StateContext, Selector } from '@ngxs/store';
import { Injectable } from '@angular/core';
import { FPayment, FAccount, FBill, AdherenceFlag, EPaymentStatus } from '../../../shared/model/account.model';
import {
  GeneratePassAction, PayAction, SkipAction, ResetAction,
  GenerateAdHocPaymentAction, UpdatePaymentTempIdAction, ClearPassAllAction
} from './pass.actions';
import { PassHttpService } from '../../../core/http/pass.http.service';
import { tap } from 'rxjs/operators';

export interface PassStateModel {
  accounts: FAccount[];
  bills: FBill[];
  payments: FPayment[];
  adherenceFlags: AdherenceFlag[];
  overdueCount: number;
  notes: string[];
  nextGeneratedId: number;  // starts at -1, decrements for each new temp ID
  loading: boolean;
  startUTC: string;
  endUTC: string;
}

const defaults: PassStateModel = {
  accounts: [], bills: [], payments: [],
  adherenceFlags: [], overdueCount: 0, notes: [],
  nextGeneratedId: -1, loading: false, startUTC: '', endUTC: ''
};

/**
 * NGXS state for the pass window. Mirrors eMAR's medpass-feature store structure.
 * Standard = accounts/bills/payments, Extra = adherence/overdue, Optional = notes.
 * NextGeneratedId gives temp negative IDs to new payments before server assigns real ones.
 */
@State<PassStateModel>({ name: 'pass', defaults })
@Injectable()
export class PassState {
  constructor(private passHttp: PassHttpService) {}

  @Selector()
  static payments(state: PassStateModel) { return state.payments; }

  @Selector()
  static accounts(state: PassStateModel) { return state.accounts; }

  @Selector()
  static bills(state: PassStateModel) { return state.bills; }

  @Selector()
  static adherenceFlags(state: PassStateModel) { return state.adherenceFlags; }

  @Selector()
  static overdueCount(state: PassStateModel) { return state.overdueCount; }

  @Selector()
  static loading(state: PassStateModel) { return state.loading; }

  @Action(GeneratePassAction)
  generate(ctx: StateContext<PassStateModel>, action: GeneratePassAction) {
    ctx.patchState({ loading: true, startUTC: action.startUTC, endUTC: action.endUTC });
    return this.passHttp.generate(action.startUTC, action.endUTC).pipe(
      tap(response => ctx.patchState({
        accounts: response.Standard.Accounts,
        bills: response.Standard.Bills,
        payments: response.Standard.Payments,
        adherenceFlags: response.Extra.AdherenceFlags,
        overdueCount: response.Extra.OverdueCount,
        notes: response.Optional.Notes,
        loading: false
      }))
    );
  }

  @Action(PayAction)
  pay(ctx: StateContext<PassStateModel>, action: PayAction) {
    ctx.patchState({
      payments: ctx.getState().payments.map(p =>
        p.PaymentId === action.paymentId
          ? { ...p, Status: EPaymentStatus.Paid, Amount: action.amount, Note: action.note, PaidAtUTC: new Date().toISOString() }
          : p
      )
    });
  }

  @Action(SkipAction)
  skip(ctx: StateContext<PassStateModel>, action: SkipAction) {
    ctx.patchState({
      payments: ctx.getState().payments.map(p =>
        p.PaymentId === action.paymentId
          ? { ...p, Status: EPaymentStatus.Skipped, Note: action.note }
          : p
      )
    });
  }

  @Action(ResetAction)
  reset(ctx: StateContext<PassStateModel>, action: ResetAction) {
    ctx.patchState({
      payments: ctx.getState().payments.map(p =>
        p.PaymentId === action.paymentId
          ? { ...p, Status: EPaymentStatus.Pending, Note: null, PaidAtUTC: null }
          : p
      )
    });
  }

  @Action(GenerateAdHocPaymentAction)
  generateAdHoc(ctx: StateContext<PassStateModel>, action: GenerateAdHocPaymentAction) {
    const state = ctx.getState();
    const tempId = state.nextGeneratedId;
    const newPayment: FPayment = {
      PaymentId: tempId,
      BillId: null,
      AccountId: action.payment.AccountId ?? 0,
      CategoryId: action.payment.CategoryId ?? 0,
      Status: EPaymentStatus.Pending,
      Amount: action.payment.Amount ?? null,
      Note: action.payment.Note ?? null,
      DueAtUTC: new Date().toISOString(),
      PaidAtUTC: null,
      ModifiedBy: 0,
      ModifiedUTC: new Date().toISOString(),
      ...action.payment
    };
    ctx.patchState({
      payments: [...state.payments, newPayment],
      nextGeneratedId: tempId - 1
    });
  }

  @Action(UpdatePaymentTempIdAction)
  updateTempId(ctx: StateContext<PassStateModel>, action: UpdatePaymentTempIdAction) {
    const payments = [...ctx.getState().payments];
    action.bindings.forEach(b => {
      const idx = payments.findIndex(p => p.PaymentId === b.TempId);
      if (idx >= 0) payments[idx] = { ...payments[idx], PaymentId: b.Id };
    });
    ctx.patchState({ payments });
  }

  @Action(ClearPassAllAction)
  clearAll(ctx: StateContext<PassStateModel>) {
    ctx.setState(defaults);
  }
}
