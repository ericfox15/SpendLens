# account.http.service.ts

**Project:** Budget.Frontend
**Layer:** Core / HTTP

## What it does

HTTP service for account CRUD operations. Extends `HttpService` to get the typed `httpGet/httpPost/httpPut/httpDelete` helpers.

```typescript
@Injectable({ providedIn: 'root' })
export class AccountHttpService extends HttpService {
  protected readonly urlBase = 'api/account';

  getAll()                           → GET  api/account
  getById(id)                        → GET  api/account/{id}
  create(account)                    → POST api/account
  update(id, account)                → PUT  api/account/{id}
  remove(id)                         → DELETE api/account/{id}
}
```

## remove() not delete()

The public method for deletion is named `remove()` because `delete` is a reserved JavaScript keyword. Subclasses can't define a public method named `delete` when the base class has a protected `httpDelete()` without TypeScript compilation errors (`TS2416`).

This naming is consistent across all HTTP services in the app.

## BaseUrlInterceptor + SecurityInterceptor

Both interceptors apply automatically to all `HttpClient` calls. `AccountHttpService` doesn't need to add the base URL or the auth token — the interceptors handle it transparently.

## Used by AccountComponent

```typescript
// In AccountComponent:
constructor(private accountHttp: AccountHttpService) {}
load() { this.accountHttp.getAll().subscribe(a => this.accounts = a); }
```
