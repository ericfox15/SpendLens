# EPaymentStatus.cs

**Project:** Budget.Application.Contracts
**Layer:** Contracts (shared across all projects)

## What it does

Defines the lifecycle states a payment can be in. Every row in `dbo.Payment` stores one of these values in the `Status` column.

```
Pending = 0   → bill was generated, waiting for user action
Paid    = 1   → user marked it paid (submitted to the API)
Skipped = 2   → user deliberately skipped this payment period
```

## eMAR pattern it mirrors

In eMAR the medication pass has a similar status column: `Pending`, `Given`, `Refused`, `Held`. The budget app collapses this to three states since there's no nursing-specific outcome needed.

## Why it lives in Application.Contracts

`Application.Contracts` is the **contract layer** — it has zero business logic and zero database knowledge. It holds only enums, options, and interfaces that any layer (API, Pass, Shared, Frontend model) can reference without pulling in heavy dependencies.

Putting enums here avoids circular project references. Budget.Api can reference `EPaymentStatus` without referencing Budget.Shared, and Budget.Pass can use it without importing Budget.Api.

## Things to notice

- Values are **not** `[Flags]` — a payment can only be in one state at a time.
- The integer backing values (`0`, `1`, `2`) are stored in SQL, so don't renumber them later or old rows break.
- The Angular frontend has a matching `EPaymentStatus` in `account.model.ts` — keep them in sync.
