# bill.component.ts

**Project:** Budget.Frontend
**Layer:** Bill CRUD

## What it does

Full CRUD page for bills — the recurring payment templates. Same inline-form pattern as `account.component.ts`.

## Additional complexity vs AccountComponent

Bills are more complex than accounts because of the scheduling fields:

### Conditional DayOfMonth field
```html
@if (form.ScheduleType === 1) {
  <div class="col-md-3">
    <label>Day of Month</label>
    <input type="number" min="1" max="31" [(ngModel)]="form.DayOfMonth" />
  </div>
}
```
`DayOfMonth` only appears when `ScheduleType = Monthly (1)`. For biweekly/weekly bills, this field is irrelevant — the schedule is determined by `AnchorDateUTC`.

### ScheduleType select

```html
<select [(ngModel)]="form.ScheduleType">
  <option [value]="1">Monthly</option>
  <option [value]="2">Biweekly</option>
  <option [value]="3">Weekly</option>
</select>
```

Note `[value]="1"` (number binding) vs `value="1"` (string binding). With `[value]`, the model gets a number; with plain `value`, it would get the string `"1"`. The C# enum values are integers, so number binding keeps types consistent.

### getScheduleName()

```typescript
getScheduleName(type: EScheduleType): string {
  return { [EScheduleType.Monthly]: 'Monthly', ... }[type] ?? '';
}
```

Uses a computed property lookup (object as a switch statement) — a compact TypeScript pattern worth knowing.

### Account and Category dropdowns

Bills form loads accounts and categories on `ngOnInit` for the dropdowns. Three parallel HTTP calls on load: bills, accounts, categories.
