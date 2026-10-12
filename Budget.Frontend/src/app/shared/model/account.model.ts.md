# account.model.ts

**Project:** Budget.Frontend
**Layer:** Shared Model

## What it does

Central TypeScript definition file for all data models, interfaces, and enums used across the Angular app. The single source of truth for the shape of data returned from the API.

## Key interfaces

| Interface | Mirrors C# type | Purpose |
|-----------|---------------|---------|
| `FAccount` | `FAccount.cs` | Account CRUD |
| `FBill` | `FBill.cs` | Bill CRUD |
| `FCategory` | `FCategory.cs` | Category CRUD |
| `FPayment` | `FPayment.cs` | Payment operations |
| `FSetting` | `FSetting.cs` | Key-value settings |
| `PassGenerateResponse` | Same | Standard/Extra/Optional buckets |
| `SubmitPayment` | Same | Per-payment submit payload |
| `PaymentBindingId` | Same | TempId → RealId mapping |
| `InitializationSettings` | Same | App startup settings |
| `ImportResult` | Same | CSV import response |
| `CategorySpendReport` | Same | Report data with percentages |
| `ActionRoute` | Same | Step pipeline configuration |

## PascalCase property names

All properties are **PascalCase** (`AccountId`, `Name`, `IsActive`) — not camelCase. This matches the `DefaultContractResolver` Newtonsoft setting on the API. If the API used camelCase, every interface property here would need to be lowercased or you'd add custom decorators.

## EScheduleType, EAccountType, EPaymentStatus

TypeScript enums with the same integer values as the C# enums:

```typescript
export enum EScheduleType {
  Monthly = 1,
  Biweekly = 2,
  Weekly = 3
}
```

Keeping the values aligned means you can compare `payment.Status === EPaymentStatus.Paid` and the number `1` means the same thing in both TypeScript and the DB.

## AdherenceFlag

A lightweight read model for the dashboard — shows a colored flag per bill indicating whether recent payments are on track, late, or auto-paid.
