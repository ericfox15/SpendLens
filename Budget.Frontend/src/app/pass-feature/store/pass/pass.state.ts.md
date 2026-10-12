# pass.state.ts

**Project:** Budget.Frontend
**Layer:** Pass Feature / NGXS Store

## What it does

The NGXS state class that holds all pass data and handles every pass action. This is the most complex piece of the Angular app.

## State shape

```typescript
export interface PassStateModel {
  Standard:  FPayment[];
  Extra:     FPayment[];
  Optional:  FPayment[];
  NextGeneratedId: number;  // starts at -1, decrements for each new ad-hoc payment
}
```

## Key reducers

### GeneratePassAction
Replaces state with the API response. Resets `NextGeneratedId` to -1.

### PayAction
```typescript
@Action(PayAction)
pay(ctx: StateContext<PassStateModel>, { payload }: PayAction) {
  const state = ctx.getState();
  const updateList = (list: FPayment[]) => list.map(p =>
    p.PaymentId === payload.payment.PaymentId
      ? { ...p, Status: EPaymentStatus.Paid, Amount: payload.amount, Note: payload.note, PaidAtUTC: new Date().toISOString() }
      : p
  );
  ctx.setState({
    ...state,
    Standard: updateList(state.Standard),
    Extra:    updateList(state.Extra),
    Optional: updateList(state.Optional)
  });
}
```

The state is **immutable** — always spread `...state` and replace specific fields. Never mutate directly.

### UpdatePaymentTempIdAction
After the API returns real IDs, replaces every negative temp ID with its real counterpart:
```typescript
bindings.forEach(b => {
  update where PaymentId === b.TempId → set PaymentId = b.RealId
});
```

### GenerateAdHocPaymentAction
Adds a new payment with a negative temp ID and decrements `NextGeneratedId`:
```typescript
ctx.patchState({
  Extra: [...state.Extra, { ...payload, PaymentId: state.NextGeneratedId }],
  NextGeneratedId: state.NextGeneratedId - 1
});
```

## Selectors

```typescript
@Selector() static standard(state: PassStateModel) { return state.Standard; }
@Selector() static extra(state: PassStateModel)    { return state.Extra; }
```

Components use `@Select(PassState.standard) standard$: Observable<FPayment[]>` to subscribe to state slices.

## eMAR parallel

eMAR's `MedPassState` follows the exact same structure. Understanding this state is directly transferable.
