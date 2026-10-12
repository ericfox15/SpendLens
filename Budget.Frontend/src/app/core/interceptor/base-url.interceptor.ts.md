# base-url.interceptor.ts

**Project:** Budget.Frontend
**Layer:** Core / HTTP Interceptor

## What it does

An Angular `HttpInterceptor` that prepends the API base URL to all relative HTTP requests.

```typescript
intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
  return from(this.configService.ready).pipe(
    switchMap(() => {
      if (req.url.startsWith('http') || req.url.startsWith('/config')) {
        return next.handle(req);  // already absolute, skip
      }
      const apiReq = req.clone({ url: this.configService.apiBaseUrl + req.url });
      return next.handle(apiReq);
    })
  );
}
```

## How interceptors work

Angular's `HttpClient` passes every outgoing request through a chain of interceptors before sending it. Each interceptor can modify the request, add headers, retry on error, or block the request entirely.

Interceptors are registered in `app-module.ts`:
```typescript
{ provide: HTTP_INTERCEPTORS, useClass: BaseUrlInterceptor, multi: true }
```

`multi: true` is required — it adds to the list of interceptors rather than replacing the list.

## Why wait for configService.ready?

The config loads asynchronously. If an HTTP request fires before the config is ready (e.g., during `APP_INITIALIZER`), `apiBaseUrl` would be empty. `from(configService.ready).pipe(switchMap(...))` waits for the Promise to resolve before continuing.

## Skipping absolute URLs and config requests

- Requests starting with `http` already have a full URL — don't prefix them.
- `/config/config.json` is a local static file, not an API call — don't prefix it.
