# category.component.ts

**Project:** Budget.Frontend
**Layer:** Category CRUD

## What it does

Full CRUD page for spending categories. Same inline-form pattern as `account.component.ts`.

## MonthlyLimit field

The main addition over the simpler account form: an optional dollar amount cap.

```html
<div class="col-md-6">
  <label>Monthly Limit (optional)</label>
  <div class="input-group">
    <span class="input-group-text">$</span>
    <input class="form-control" type="number" [(ngModel)]="form.MonthlyLimit" />
  </div>
</div>
```

`MonthlyLimit` maps to `FCategory.MonthlyLimit` (nullable decimal). Leaving it blank sends `null` to the API, which stores `NULL` in the DB. The report page skips the over-budget warning for categories with `null` limits.

## 10 seed categories

The database migration pre-seeds these categories:
- Housing, Utilities, Groceries, Transportation, Health, Entertainment, Subscriptions, Dining, Savings, Miscellaneous

They appear in the list immediately. Users can edit limits or add their own custom categories.

## Category CRUD is used by Bills

Categories appear in the bill form's "Category" dropdown. If you add a new category here, it immediately shows in the bill form. Both components load categories fresh from the API on each `ngOnInit`.
