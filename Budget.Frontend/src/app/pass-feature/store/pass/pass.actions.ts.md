# pass.actions.ts

**Project:** Budget.Frontend
**Layer:** Pass Feature / NGXS Store

## What it does

Defines all the action classes for the NGXS pass state. Actions are like events — they describe what happened, and the state handler (`PassState`) decides how to update state in response.

## Actions defined

| Action | Payload | Purpose |
|--------|---------|---------|
| `GeneratePassAction` | `PassGenerateResponse` | Load generated payments into state |
| `PayAction` | `{ payment, amount, note }` | Mark a payment as Paid, update amount |
| `SkipAction` | `{ paymentId }` | Mark a payment as Skipped |
| `ResetAction` | `{ paymentId }` | Reset a payment back to Pending |
| `GenerateAdHocPaymentAction` | `SubmitPayment` | Add a one-off payment to the Extra bucket |
| `UpdatePaymentTempIdAction` | `PaymentBindingId[]` | Replace temp negative IDs with real DB IDs |
| `ClearPassAllAction` | none | Clear all pass data (on logout or new generate) |

## NGXS action pattern

```typescript
export class PayAction {
  static readonly type = '[Pass] Pay';
  constructor(public payload: { payment: FPayment; amount: number; note: string }) {}
}
```

- `type` must be unique across the entire app. Convention: `[FeatureName] ActionDescription`.
- The constructor payload carries all data the state handler needs.
- Actions are dispatched from components:
  ```typescript
  this.store.dispatch(new PayAction({ payment, amount, note }));
  ```

## Why NGXS instead of a plain service?

State management solves the "shared mutable state" problem. Multiple components need to see the same pass data:
- `PassComponent` renders it
- `SubmitQueueService` reads it for offline submission
- `DashboardComponent` shows a summary

Without a store, you'd pass data via `@Input`/`@Output` chains or a service with `BehaviorSubject`. NGXS gives a single, inspectable state tree with Redux DevTools support.

## eMAR parallel

eMAR's medpass feature uses the exact same NGXS action pattern: `GenerateMedPassAction`, `GiveMedicationAction`, `RefuseMedicationAction`, `UpdateMedPassTempIdAction`.
