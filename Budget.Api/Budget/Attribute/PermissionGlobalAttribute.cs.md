# PermissionGlobalAttribute.cs

**Project:** Budget.Api
**Layer:** Global Action Filter

## What it does

A global `IActionFilter` that runs after every action is selected but before it executes. Enforces the `[Permission]` attribute system.

## Execution flow

```
OnActionExecuting(context):
  1. Check if action has [PermissionSkip] → return (skip check)
  2. Check if action has [Permission] attribute
     - If no attribute → still require *some* valid token (default: Read)
  3. Read JWT claim "perm" from HttpContext.User
     - If claim missing → 401 Unauthorized
  4. Parse claim as long (bit-flag)
  5. Check: (userBits & (long)attr.Required) != 0
     - If false → 403 Forbidden
     - If true → continue to action
```

## Global registration

Registered for all controllers in `Program.cs`:

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<PermissionGlobalAttribute>();
    options.Filters.Add<AuditRequestGlobalAttribute>();
});
```

By registering globally, every controller action is covered automatically. You never forget to add `[Permission]` on a new endpoint.

## Important: runs AFTER JWT validation

ASP.NET Core's JWT middleware validates the token signature and expiry first. If the token is expired or tampered, the request is rejected before `PermissionGlobalAttribute` even runs. By the time this filter sees the request, the JWT is already verified — it only needs to check the claims.
