# ControllerReport.cs

**Project:** Budget.Api
**Layer:** Controller (HTTP Entry Point)

## What it does

Provides spending reports. Currently one endpoint: category spend for a given month.

| Method | Route | Query Params | Description |
|--------|-------|-------------|-------------|
| GET | `api/report/category-spend` | `year`, `month` | Spending by category with percentages |

## Category spend calculation

```csharp
var payments = await _repo.SelectPaymentsForMonth(year, month);
var totalSpend = payments.Sum(p => p.Amount);

var report = payments
    .GroupBy(p => p.CategoryId)
    .Select(g => new CategorySpendReport
    {
        CategoryId   = g.Key,
        CategoryName = g.First().CategoryName,
        TotalSpend   = g.Sum(p => p.Amount),
        Percentage   = totalSpend == 0 ? 0 : (g.Sum(p => p.Amount) / totalSpend) * 100,
        MonthlyLimit = g.First().MonthlyLimit
    });
```

The **percentage** answers: "Of everything I spent this month, what fraction went to groceries?" — the core feature the user requested.

## Angular report component

`ReportComponent` calls this endpoint, then renders a Bootstrap progress bar for each category where `width = Percentage%`. If `TotalSpend > MonthlyLimit`, the bar turns red to indicate over-budget.

## Direct repository access (no service)

The report is read-only with no business logic — just aggregate SQL + percentage math. Following the eMAR pattern for simple reads, the controller calls the report repository directly without a service layer.
