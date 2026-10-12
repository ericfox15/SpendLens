# category.http.service.ts

**Project:** Budget.Frontend
**Layer:** Core / HTTP

## What it does

HTTP service for category CRUD. Identical structure to `AccountHttpService`.

```typescript
@Injectable({ providedIn: 'root' })
export class CategoryHttpService extends HttpService {
  protected readonly urlBase = 'api/category';

  getAll()                → GET  api/category
  getById(id)             → GET  api/category/{id}
  create(category)        → POST api/category
  update(id, category)    → PUT  api/category/{id}
  remove(id)              → DELETE api/category/{id}
}
```

## Used in two places

1. **CategoryComponent** — the CRUD management page
2. **BillComponent** — loads all categories for the "Category" dropdown when creating/editing a bill

Because Angular uses singleton services by default (`providedIn: 'root'`), both components share the same `CategoryHttpService` instance without double-creating it.
