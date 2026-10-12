# dashboard.component.ts

**Project:** Budget.Frontend
**Layer:** Dashboard

## What it does

The home page. Shows two key widgets:
1. **Overdue payments** — bills past due and still Pending
2. **Adherence flags** — per-bill indicators showing recent payment history (on-time, late, auto-paid, etc.)

## Overdue widget

```typescript
this.overdueHttp.getOverdue(0, 5).subscribe(payments => this.overdue = payments);
```

Calls `GET api/overdue/payments?skip=0&take=5`. Shows the top 5 most overdue items. Each row shows the bill name, due date, and amount.

## Adherence flags

```typescript
// Called via IRepositoryPassGenerate.SelectAdherenceFlags()
this.passHttp.generate(start, end).subscribe(response => {
  // extract adherence from response metadata
});
```

An `AdherenceFlag` has:
```typescript
interface AdherenceFlag {
  BillId: number;
  BillName: string;
  FlagType: 'OnTime' | 'Late' | 'AutoPay' | 'Skipped' | 'NeverPaid';
}
```

Color-coded badges give the user a quick visual health check of their payment habits.

## Bootstrap cards

Uses Bootstrap 5 card components for the widget layout. The overdue widget renders a mini-table inside a card; adherence flags render inline badge pills.

## Refresh on init

Both widgets load on `ngOnInit()` and can be manually refreshed. The dashboard is intentionally read-only — no actions here, just at-a-glance status.
