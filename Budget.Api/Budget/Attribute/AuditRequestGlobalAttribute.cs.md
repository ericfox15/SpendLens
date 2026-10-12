# AuditRequestGlobalAttribute.cs

**Project:** Budget.Api
**Layer:** Global Action Filter (Audit)

## What it does

A global `IAsyncActionFilter` that logs every API request to `dbo.AuditLog` **after** the action completes successfully. Think of it as a lightweight access log inside the database.

## What gets logged

```
UserId      → from JWT claim (null for anonymous)
Username    → from JWT claim
Action      → controller + action name (e.g., "ControllerBill.Create")
Path        → request URL path
StatusCode  → HTTP response status code
RequestedAt → UTC timestamp
```

## Why after the action, not before?

Logging after the action lets you capture the HTTP status code — so you can tell whether the action succeeded (200/201) or failed (400/500). Logging before would give you only "this was attempted."

## Critical: never fail the request

The entire filter is wrapped in a `try/catch` that swallows all exceptions:

```csharp
try { await _repo.InsertAuditLog(entry); }
catch { /* intentionally swallowed */ }
```

If the audit log INSERT fails (e.g., DB table missing, deadlock), the actual API response must still be returned to the client. Audit logging is observability, not business logic — it must never break the user's experience.

## IAsyncActionFilter vs IActionFilter

`IAsyncActionFilter` lets you `await` async operations (like the DB insert). The `IActionFilter` sync version can't await — you'd need `.Wait()` which risks deadlocks.
