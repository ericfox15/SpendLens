# config.service.ts

**Project:** Budget.Frontend
**Layer:** Shared / Config

## What it does

Loads `src/config/config.json` at app startup and exposes configuration values to the rest of the app.

```typescript
@Injectable({ providedIn: 'root' })
export class ConfigService {
  apiBaseUrl = '';
  authenticationMode = 'jwt';

  ready: Promise<void>;

  load(): Promise<void> {
    this.ready = fetch('/config/config.json')
      .then(r => r.json())
      .then(config => {
        this.apiBaseUrl = config.apiBaseUrl;
        this.authenticationMode = config.authenticationMode;
      });
    return this.ready;
  }
}
```

`config/config.json`:
```json
{ "apiBaseUrl": "http://localhost:5001/", "authenticationMode": "jwt" }
```

## Why a JSON config file instead of environment.ts?

Angular's `environment.ts` is baked in at build time. `config.json` is served as a static file — different environments can use different configs without rebuilding the app. In Docker, you'd mount a different `config.json` per environment.

## The ready: Promise<void> pattern

`BaseUrlInterceptor` awaits `configService.ready` before prepending the base URL to HTTP requests. This ensures that even if the interceptor runs before `load()` completes, it waits for the config to be available.

## APP_INITIALIZER

In `app-module.ts`:
```typescript
provideAppInitializer(() => inject(ConfigService).load())
```

This tells Angular: "before rendering anything, wait for `ConfigService.load()` to complete." If the config fetch fails, the app won't start.
