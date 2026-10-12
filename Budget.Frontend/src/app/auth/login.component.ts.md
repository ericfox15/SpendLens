# login.component.ts

**Project:** Budget.Frontend
**Layer:** Auth

## What it does

Simple login form. Sends username + password to `api/authentication/login`, stores the returned JWT token in `localStorage`, then redirects to the dashboard.

## Template

```html
<form>
  <input [(ngModel)]="username" placeholder="Username" />
  <input [(ngModel)]="password" type="password" placeholder="Password" />
  <button (click)="login()">Login</button>
  @if (error) { <div class="alert alert-danger">{{ error }}</div> }
</form>
```

## login() method

```typescript
login(): void {
  this.authHttp.login(this.username, this.password).subscribe({
    next: (res) => {
      localStorage.setItem('budget_token', res.Token);
      this.router.navigate(['/dashboard']);
    },
    error: () => {
      this.error = 'Invalid username or password.';
    }
  });
}
```

## Security note

The error message is deliberately vague — "Invalid username or password" rather than "User not found" or "Wrong password". This prevents user enumeration (matching the server-side behavior in `ControllerAuthentication`).

## Token storage

`budget_token` is the localStorage key read by `SecurityInterceptor`. Once stored, all subsequent HTTP requests automatically include the `Authorization: Bearer <token>` header.

## No route guard yet

After login, the user is redirected manually via `router.navigate`. A proper `AuthGuard` would protect routes and redirect unauthenticated users to `/login` automatically. This is a straightforward improvement to add.
