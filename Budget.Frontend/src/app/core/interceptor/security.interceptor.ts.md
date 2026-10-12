# security.interceptor.ts

**Project:** Budget.Frontend
**Layer:** Core / HTTP Interceptor

## What it does

Adds the JWT Bearer token to every outgoing HTTP request's `Authorization` header.

```typescript
intercept(req: HttpRequest<any>, next: HttpHandler) {
  const token = localStorage.getItem('budget_token');
  if (!token) return next.handle(req);

  const authReq = req.clone({
    headers: req.headers.set('Authorization', `Bearer ${token}`)
  });
  return next.handle(authReq);
}
```

## HttpRequest.clone()

Angular's `HttpRequest` objects are **immutable** — you can't modify them directly. `clone()` creates a new request with the specified modifications applied. This is important for interceptor chaining: each interceptor in the chain sees the original request or a previous interceptor's clone.

## localStorage key: budget_token

`LoginComponent` stores the token:
```typescript
localStorage.setItem('budget_token', response.Token);
```

And removes it on logout (in `AppComponent`):
```typescript
localStorage.removeItem('budget_token');
```

## Security note

Storing JWTs in `localStorage` is convenient but has an XSS risk: malicious scripts on the page can read `localStorage`. An httpOnly cookie is more secure but harder to implement with CORS. For a personal budgeting app, `localStorage` is an acceptable tradeoff. In a high-security context, use httpOnly cookies.

## Two interceptors, one chain

`BaseUrlInterceptor` runs first (adds the full URL), then `SecurityInterceptor` runs (adds the token). Both are registered in `app-module.ts` in order.
