# http.service.ts

**Project:** Budget.Frontend
**Layer:** Core / HTTP

## What it does

Abstract base class for all HTTP service classes. Provides typed wrappers around `HttpClient` methods.

```typescript
export abstract class HttpService {
  protected abstract readonly urlBase: string;
  constructor(protected http: HttpClient) {}

  protected httpGet<T>(path = ''): Observable<T> {
    return this.http.get<T>(this.urlBase + path);
  }
  protected httpPost<T, B>(path: string, body: B): Observable<T> {
    return this.http.post<T>(this.urlBase + path, body);
  }
  protected httpPut<B>(path: string, body: B): Observable<void> {
    return this.http.put<void>(this.urlBase + path, body);
  }
  protected httpDelete(path: string): Observable<void> {
    return this.http.delete<void>(this.urlBase + path);
  }
}
```

## Why httpGet/httpPost instead of get/post?

The original names (`get`, `post`, `delete`) conflict with JavaScript built-in or common method names. Subclasses that defined `delete(id: number)` would shadow the protected `delete()` from the base class, causing TypeScript errors `TS2416` and `TS7023`. Prefixing with `http` avoids the conflict while keeping the intent clear.

Subclass public methods use `remove()` instead of `delete()` for the same reason.

## urlBase pattern

Each concrete service sets `urlBase`:
```typescript
export class BillHttpService extends HttpService {
    protected readonly urlBase = 'api/bill';
}
```

`BaseUrlInterceptor` prepends the full API host, so the final URL becomes `http://localhost:5001/api/bill`.

## Mirrors C# FRepositoryBase

This plays the same role as `FRepositoryBase.cs` on the backend: a base class that provides connection infrastructure, letting subclasses focus on their specific calls without repeating boilerplate.
