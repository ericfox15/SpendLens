# ControllerImport.cs

**Project:** Budget.Api
**Layer:** Controller (HTTP Entry Point)

## What it does

Handles bank statement CSV upload and auto-categorization. The feature the user originally asked for: "inputted a monthly transaction bank statement and then show the user the percentages they spend on each thing."

| Method | Route | Body | Description |
|--------|-------|------|-------------|
| POST | `api/import/bank-statement` | `multipart/form-data` | Upload CSV, parse, auto-categorize, return spending breakdown |

## How it works

```
1. Receive multipart file upload
2. Read CSV lines with CsvHelper (or simple string parsing)
3. For each transaction row:
   a. Parse: date, description, amount
   b. Keyword match: scan description against 50+ keyword rules → assign CategoryId
   c. Create FPayment record as ad-hoc (BillId = null)
4. Bulk INSERT all payments
5. Calculate spend by category (same as /report/category-spend)
6. Return ImportResult { ImportedCount, SpendByCategory[] }
```

## Keyword auto-categorization

The controller has a static dictionary of 50+ keyword → category mappings:

```csharp
private static readonly Dictionary<string, int> _keywords = new()
{
    { "netflix",   /* Subscriptions CategoryId */ },
    { "spotify",   /* Subscriptions CategoryId */ },
    { "walmart",   /* Groceries CategoryId */ },
    { "kroger",    /* Groceries CategoryId */ },
    { "chevron",   /* Transportation CategoryId */ },
    // ... 45+ more
};
```

The match is case-insensitive. If no keyword matches, the transaction falls into "Miscellaneous."

## ImportResult response

```csharp
public class ImportResult
{
    public int ImportedCount { get; set; }
    public List<CategorySpendReport> SpendByCategory { get; set; }
}
```

The Angular `ImportComponent` renders this with the same progress-bar visualization as the report page.

## CSV format assumption

The importer expects standard bank export format: `Date, Description, Amount` columns. Different banks have slightly different formats — the current parser handles the most common layout (Chase, Bank of America style).
