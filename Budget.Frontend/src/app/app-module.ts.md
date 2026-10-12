# app-module.ts

**Project:** Budget.Frontend
**Layer:** Root NgModule

## What it does

The root Angular module. Declares all components, imports all modules, registers providers (DI), and wires everything together. In a standalone-component app this would be replaced by `bootstrapApplication()`, but this project uses NgModule architecture to mirror eMAR.

## Key imports

| Module | Purpose |
|--------|---------|
| `FormsModule` | Two-way `[(ngModel)]` binding in forms |
| `HttpClientModule` | Angular's `HttpClient` (required for HTTP services) |
| `NgbModule` | ng-bootstrap components (modals, badges, etc.) |
| `NgxsModule.forRoot([PassState])` | NGXS store with PassState slice |
| `NgxsReduxDevtoolsPluginModule` | Connect NGXS to Redux DevTools browser extension |
| `ToastrModule.forRoot()` | ngx-toastr notification service |
| `BrowserAnimationsModule` | Required by Toastr for animations |

## HTTP_INTERCEPTORS

```typescript
{ provide: HTTP_INTERCEPTORS, useClass: BaseUrlInterceptor, multi: true },
{ provide: HTTP_INTERCEPTORS, useClass: SecurityInterceptor, multi: true },
```

Order matters — `BaseUrlInterceptor` runs first (prepends base URL), then `SecurityInterceptor` (adds token).

## APP_INITIALIZER

```typescript
provideAppInitializer(() => inject(ConfigService).load())
```

Angular calls this function during bootstrapping and waits for the returned Promise before rendering the app. Guarantees `ConfigService.apiBaseUrl` is populated before any HTTP requests fire.

## standalone: false

All components declare `standalone: false`. Angular 22 defaults to standalone; this was explicitly overridden to use NgModule architecture, matching the eMAR pattern.

## Routing

`AppRoutingModule` is imported here. Routes are defined in `app-routing-module.ts`.
