# FAccount.cs

**Project:** Budget.Shared
**Layer:** Domain Model

## What it does

The canonical domain object for a bank/credit account. Maps directly to the `dbo.Account` table.

| Property | SQL Column | Notes |
|----------|-----------|-------|
| `AccountId` | `AccountId INT PK` | Auto-generated identity |
| `Name` | `Name NVARCHAR(100)` | Display label (e.g., "Chase Checking") |
| `AccountType` | `AccountType INT` | Stored as `EAccountType` integer |
| `IsActive` | `IsActive BIT` | Soft-delete flag |

## F* naming convention

`F` prefix = **domain/frontend model** in eMAR convention. These are the "fat" objects that move between layers — from the database up through the service and controller to the Angular frontend as JSON.

Contrast with `S*` models (summary/read-only projections — fewer columns, possibly joined from multiple tables) and `E*` enums.

## Soft delete

`IsActive = 0` means deleted. No rows are ever physically removed from `dbo.Account`. This is important because:
1. Payments reference `AccountId` — deleting an account would require cascading deletes or FK violations.
2. Audit trail: you can always see what existed.

All SELECT queries filter `WHERE IsActive = 1`. The DELETE endpoints in the API set `IsActive = 0` instead of running `DELETE FROM`.

## Angular equivalent

`FAccount` in TypeScript (`account.model.ts`) mirrors this structure exactly. The JSON property names are PascalCase because `Program.cs` configures Newtonsoft.Json with `DefaultContractResolver` (no camelCase conversion).
