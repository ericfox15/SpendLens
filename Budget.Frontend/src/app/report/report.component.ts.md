# report.component.ts

**Project:** Budget.Frontend
**Layer:** Report

## What it does

Displays a monthly spending breakdown by category with progress bars showing what percentage of total spend went to each category.

## Template (key part)

```html
@for (item of report; track item.CategoryId) {
  <div class="mb-3">
    <div class="d-flex justify-content-between">
      <span>{{ item.CategoryName }}</span>
      <span>${{ item.TotalSpend | number:'1.2-2' }} ({{ item.Percentage | number:'1.0-1' }}%)</span>
    </div>
    <div class="progress">
      <div class="progress-bar"
           [class.bg-danger]="item.TotalSpend > item.MonthlyLimit"
           [class.bg-warning]="isNearLimit(item)"
           [style.width.%]="item.Percentage">
      </div>
    </div>
  </div>
}
```

## Year/month picker

```typescript
year  = new Date().getFullYear();
month = new Date().getMonth() + 1;

load() {
  this.reportHttp.getCategorySpend(this.year, this.month)
    .subscribe(data => this.report = data);
}
```

Defaults to current month. User can navigate to previous months.

## isNearLimit(item)

Returns `true` when spend is past the `OverBudgetWarnPercent` threshold (loaded from initialization settings) but not yet over the limit. Shows yellow warning progress bar.

## The "percentage of spend" feature

`item.Percentage` is calculated server-side in `ControllerReport`:
```
Percentage = (categoryTotalSpend / totalMonthlySpend) * 100
```

This answers: "Of all the money I spent in October, 34% went to Rent, 12% to Groceries, etc." — the core insight the user requested.
