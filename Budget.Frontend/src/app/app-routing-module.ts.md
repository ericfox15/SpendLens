# app-routing-module.ts

**Project:** Budget.Frontend
**Layer:** Routing

## What it does

Defines the client-side routes for the Angular SPA (Single Page Application).

| Path | Component | Description |
|------|-----------|-------------|
| `login` | `LoginComponent` | Authentication page |
| `dashboard` | `DashboardComponent` | Overview: overdue + adherence |
| `pass` | `PassComponent` | The bill payment pass |
| `accounts` | `AccountComponent` | Account CRUD |
| `bills` | `BillComponent` | Bill CRUD |
| `categories` | `CategoryComponent` | Category CRUD |
| `report` | `ReportComponent` | Category spend report |
| `import` | `ImportComponent` | Bank statement CSV upload |
| `''` | redirect | Root → dashboard |

## SPA routing

Angular handles routing **client-side** — no page reloads. When you navigate to `/pass`, Angular renders `PassComponent` without talking to the server. The URL changes via the History API.

The API serves `index.html` as a fallback for any unrecognized path (`app.MapFallbackToFile("index.html")`), so refreshing the browser on any route still loads the Angular app (which then handles the route itself).

## Guards (future improvement)

Currently there's no `AuthGuard` — routes don't redirect to login if the user has no token. Adding a guard would be:
```typescript
{ path: 'pass', component: PassComponent, canActivate: [AuthGuard] }
```

The `SecurityInterceptor` protects the data (all API calls return 401), but the UI doesn't redirect automatically. This is a good learning exercise to add.
