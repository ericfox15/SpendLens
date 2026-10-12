# import.component.ts

**Project:** Budget.Frontend
**Layer:** Import

## What it does

The bank statement CSV import page. User uploads their bank's transaction export, the server auto-categorizes each transaction, and the component displays a spending breakdown identical to the report page.

## Template flow

```
1. [Choose File] button → file input (hidden, triggered by label)
2. Selected file name shown
3. [Upload & Analyze] button → calls importBankStatement()
4. @if (result) {
     Imported {{ result.ImportedCount }} transactions
     [same progress-bar breakdown as report page]
   }
```

## importBankStatement()

```typescript
import() {
  if (!this.selectedFile) return;
  this.importHttp.importBankStatement(this.selectedFile)
    .subscribe(result => this.result = result);
}
```

The file is sent as `multipart/form-data`. `ImportHttpService` uses the raw `this.http.post()` with a `FormData` object (the base class helper doesn't support file uploads).

## Why multipart/form-data for file upload?

Binary file data can't be sent as JSON. `FormData` encodes the file as a multipart body, which ASP.NET Core reads with `IFormFile file` in the controller action parameter.

## Auto-categorization result

`ImportResult.SpendByCategory` uses the same `CategorySpendReport` structure as the report page — same progress bars, same percentage display. After importing, users immediately see where their money went.

## Supported CSV format

The importer expects: `Date, Description, Amount` — the standard format exported by most US banks (Chase, Bank of America, Wells Fargo). The keyword matching covers 50+ common payee names for auto-categorization.
