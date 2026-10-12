# app.ts (AppComponent)

**Project:** Budget.Frontend
**Layer:** Root Component

## What it does

The root component rendered for every page. Contains the Bootstrap navbar with navigation links and login state management.

## Template structure

```
<nav class="navbar">           ← Bootstrap top nav
  <a routerLink="/dashboard">  ← Logo/brand link
  @if (isLoggedIn) {
    [nav links: Dashboard, Pass, Accounts, Bills, Categories, Report, Import]
    <button (click)="logout()">
  } @else {
    <a routerLink="/login">
  }
</nav>
<router-outlet></router-outlet>  ← current route renders here
```

## Login state

```typescript
get isLoggedIn(): boolean {
  return !!localStorage.getItem('budget_token');
}

logout(): void {
  localStorage.removeItem('budget_token');
  this.router.navigate(['/login']);
}
```

Reads `localStorage` directly — no separate auth service. Simple and sufficient for a personal app.

## @if control flow

Uses Angular 17+ `@if` / `@for` control flow syntax (not `*ngIf` / `*ngFor` directives). The new syntax is cleaner and slightly more performant.

## router-outlet

`<router-outlet>` is a placeholder where `AppRoutingModule` renders the matched route component. `AppComponent` itself is always visible (the navbar persists); only the content inside `router-outlet` changes on navigation.
