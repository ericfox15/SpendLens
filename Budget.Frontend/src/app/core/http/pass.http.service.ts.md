# pass.http.service.ts

**Project:** Budget.Frontend
**Layer:** Core / HTTP

## What it does

Contains multiple HTTP service classes for non-CRUD endpoints. Grouped in one file because they're all closely related to the pass workflow and initialization.

## Services in this file

### PassHttpService
`urlBase = 'api/pass'`

| Method | Calls | Description |
|--------|-------|-------------|
| `generate(startUTC, endUTC)` | POST /generate | Generate payment pass |
| `submitPayments(payments, isOfflineSync)` | POST /submit/payment | Submit payment batch |

### InitializationHttpService
`urlBase = 'api/core/initialization'`

| Method | Calls | Description |
|--------|-------|-------------|
| `getSettings()` | GET /settings | Load ActionRoutes + config flags |

### ReportHttpService
`urlBase = 'api/report'`

| Method | Calls | Description |
|--------|-------|-------------|
| `getCategorySpend(year, month)` | GET /category-spend | Monthly spending by category |

### OverdueHttpService
`urlBase = 'api/overdue'`

| Method | Calls | Description |
|--------|-------|-------------|
| `getOverdue(skip, take)` | GET /payments | Paginated overdue payments |

### AuthHttpService
`urlBase = 'api/authentication'`

| Method | Calls | Description |
|--------|-------|-------------|
| `login(username, password)` | POST /login | Returns JWT token |

### ImportHttpService
`urlBase = 'api/import'`

| Method | Calls | Description |
|--------|-------|-------------|
| `importBankStatement(file)` | POST /bank-statement | CSV upload, returns ImportResult |

Note: `importBankStatement` uses `this.http.post` directly (not the base class helper) because it sends `FormData` for file upload, not JSON.
