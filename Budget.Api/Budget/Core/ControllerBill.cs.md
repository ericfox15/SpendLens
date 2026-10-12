# ControllerBill.cs

**Project:** Budget.Api
**Layer:** Controller (HTTP Entry Point)

## What it does

CRUD endpoints for bills (recurring payment templates).

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| GET | `api/bill` | Read | Get all active bills (with account/category names) |
| GET | `api/bill/{id}` | Read | Get bill by ID |
| POST | `api/bill` | Write | Create new bill |
| PUT | `api/bill/{id}` | Write | Update bill |
| DELETE | `api/bill/{id}` | Write | Deactivate bill (soft delete) |

## Bills vs Payments

`ControllerBill` manages bill **templates** — the schedule definitions. It does not manage actual payments. When a bill is deactivated, future pass generations skip it, but existing payments for that bill remain in the database.

## Same pattern as ControllerAccount

Simple reads call the repository directly; writes go through the service. The service handles the `AnchorDateUTC` defaulting for biweekly/weekly bills (see `ServiceBill.cs.md`).
