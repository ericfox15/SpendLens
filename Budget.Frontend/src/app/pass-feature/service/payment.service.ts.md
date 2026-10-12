# payment.service.ts

**Project:** Budget.Frontend
**Layer:** Pass Feature / Service

## What it does

Orchestrates the entire "pay a bill" user flow using the configurable action-route pipeline. This is the most important Angular service to understand.

## runAction(payment, routes)

The core method. Takes the payment being submitted and the enabled action routes (sorted by `Order`). Executes each step in sequence.

```typescript
async runAction(payment: FPayment, routes: ActionRoute[]): Promise<void> {
  for (const route of routes) {
    switch (route.StepType) {
      case 'CHECK_DUPLICATE':
        // check if payment already paid, prompt user if so
        break;
      case 'GET_AMOUNT':
        // show modal/prompt for actual amount (may differ from expected)
        break;
      case 'GET_NOTE':
        // show modal/prompt for optional note
        break;
      case 'SUBMIT_PAYMENT':
        // call PassHttpService.submitPayments(), dispatch UpdatePaymentTempIdAction
        break;
      case 'KILL':
        // abort the flow (used as a circuit-breaker step)
        return;
    }
  }
}
```

## Why this design matters

Without the pipeline, `runAction()` would be a hardcoded sequence of if-statements. That works for one facility/user, but breaks when you need different workflows:
- Desktop: show amount modal
- Mobile: skip amount modal (just use expected amount)
- Power user: no confirmation dialogs at all

By loading the steps from `dbo.Setting`, each configuration gets its own behavior without duplicating code.

## Integration with NGXS

After a successful submit, `PaymentService` dispatches:
```typescript
this.store.dispatch(new PayAction({ payment, amount, note }));           // optimistic update
// ... after API responds:
this.store.dispatch(new UpdatePaymentTempIdAction(bindingIds));           // replace temp IDs
```

The optimistic update makes the UI feel instant — the payment shows as Paid immediately, before the API call completes.

## Offline handling

Before calling the API, `PaymentService` checks `navigator.onLine`. If offline, it adds the payment to the Dexie queue (`queueSubmitPayment`) instead of calling the API. `SubmitQueueService` will drain it when connectivity returns.
