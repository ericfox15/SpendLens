# pass.component.ts

**Project:** Budget.Frontend
**Layer:** Pass Feature / Component

## What it does

The main pass page — where users mark bills as paid or skipped for a time window. This is the core feature of the app.

## Layout

Three sections using the `PassGenerateResponse` structure:

```
Standard (regular bills)
  ├── Rent          [Pay] [Skip]
  ├── Electric      [Pay] [Skip]  ← already Paid: green badge
  └── Netflix       [Pay] [Skip]

Extra (ad-hoc payments)
  ├── Doctor visit  [Pay] [Skip]
  └── [+ Add]                    ← GenerateAdHocPaymentAction

Optional (auto-pay)
  └── Car Insurance ← auto-paid, no buttons
```

## NGXS integration

```typescript
@Select(PassState.standard) standard$!: Observable<FPayment[]>;
@Select(PassState.extra)    extra$!:    Observable<FPayment[]>;
@Select(PassState.optional) optional$!: Observable<FPayment[]>;

pay(payment: FPayment) {
  this.paymentService.runAction(payment, this.actionRoutes);
}

skip(payment: FPayment) {
  this.store.dispatch(new SkipAction({ paymentId: payment.PaymentId }));
  // also calls API to persist
}
```

## Action routes from initialization

`ActionRoutes` are loaded from `InitializationHttpService` at startup and passed to `PaymentService.runAction()`. The pass component doesn't know what steps run — it just delegates to the service.

## Date window picker

The user selects a start/end date range at the top of the page, then clicks "Generate". This calls `POST api/pass/generate` and dispatches `GeneratePassAction` with the response.

## Visual status indicators

| Status | Display |
|--------|---------|
| Pending | White row, Pay/Skip buttons visible |
| Paid | Green badge, "Paid $XX.XX" label |
| Skipped | Gray badge, "Skipped" label |
