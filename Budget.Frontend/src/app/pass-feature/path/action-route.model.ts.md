# action-route.model.ts

**Project:** Budget.Frontend
**Layer:** Pass Feature / Action-Route Pipeline

## What it does

Defines the TypeScript model for the **configurable action-route pipeline** — the most architecturally interesting part of the app.

```typescript
export type StepType =
  | 'CHECK_DUPLICATE'
  | 'CHECK_OVERDUE'
  | 'GET_AMOUNT'
  | 'GET_NOTE'
  | 'SUBMIT_PAYMENT'
  | 'KILL';

export interface ActionRoute {
  StepType: StepType;
  Order: number;
  IsEnabled: boolean;
}
```

## What is the action-route pipeline?

When the user clicks "Pay" on a bill, `PaymentService.runAction()` doesn't hardcode what happens next. Instead, it reads the `ActionRoutes` loaded from the API at startup and executes each enabled step in order.

Example configuration (stored in `dbo.Setting`):
```json
[
  { "StepType": "CHECK_DUPLICATE", "Order": 1, "IsEnabled": true },
  { "StepType": "GET_AMOUNT",      "Order": 2, "IsEnabled": true },
  { "StepType": "SUBMIT_PAYMENT",  "Order": 3, "IsEnabled": true }
]
```

To add a confirmation modal before submit, you just add `{ "StepType": "CHECK_CONFIRM", "Order": 2.5, ... }` in the database. **No frontend redeploy needed.**

## Helper functions

### isValidActionRoute(step: string): step is StepType
Type guard — confirms a string is one of the valid step types. Used when parsing the API response.

### parseActionRoutes(raw: ActionRoute[]): ActionRoute[]
Filters to enabled routes and sorts by `Order`. Returns a clean, ready-to-execute list.

## eMAR parallel

eMAR uses this exact pattern for its medication administration workflow. The steps (check allergies, confirm dose, document time, etc.) are DB-configurable per facility. A facility can enable or disable steps without a code change. This is the key pattern the entire app was built to teach.
