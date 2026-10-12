# bill.http.service.ts

**Project:** Budget.Frontend
**Layer:** Core / HTTP

## What it does

HTTP service for bill CRUD. Same structure as the other CRUD HTTP services.

```typescript
@Injectable({ providedIn: 'root' })
export class BillHttpService extends HttpService {
  protected readonly urlBase = 'api/bill';

  getAll()              → GET  api/bill           (includes AccountName, CategoryName)
  getById(id)           → GET  api/bill/{id}
  create(bill)          → POST api/bill           (returns { BillId: number })
  update(id, bill)      → PUT  api/bill/{id}
  remove(id)            → DELETE api/bill/{id}
}
```

## create() returns an ID

The API returns `{ BillId: number }` on create — not the full bill object. This is efficient: the component already has the data it just submitted; it just needs to know the assigned ID. After creating, `BillComponent` calls `load()` to refresh the full list from the server.

## getAll() returns joined data

The server-side JOIN on `dbo.Account` and `dbo.Category` means `FBill.AccountName` and `FBill.CategoryName` are populated in the list view without additional requests.
