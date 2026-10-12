# FCategory.cs

**Project:** Budget.Shared
**Layer:** Domain Model

## What it does

Represents a spending category. Maps to `dbo.Category`.

| Property | Notes |
|----------|-------|
| `CategoryId` | Identity PK |
| `Name` | Label shown in UI (e.g., "Rent", "Groceries", "Utilities") |
| `MonthlyLimit` | Optional budget cap in dollars. Used by reports to flag over-budget spend. |

## Seed data

The database migration (`0001_BUDGET_Init.sql`) seeds 10 default categories:

- Housing, Utilities, Groceries, Transportation, Health, Entertainment, Subscriptions, Dining, Savings, Miscellaneous

You can add your own via the Categories CRUD page in the frontend.

## How MonthlyLimit is used

`ControllerReport.GetCategorySpend()` calculates total spend per category for a given month and compares it against `MonthlyLimit`. If spend exceeds the limit, the frontend can show a warning indicator. The report also calculates the **percentage** of total monthly spend each category represents.

`RepositoryConfiguration` (Settings service) also has a `GetOverBudgetWarnPercent()` method — a second threshold (e.g., 80%) at which to show a yellow warning before the red "over budget" state.
