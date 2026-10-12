# PermissionSkipAttribute.cs

**Project:** Budget.Api
**Layer:** Attribute / Authorization

## What it does

An opt-out marker that tells `PermissionGlobalAttribute` to skip the permission check for this endpoint.

```csharp
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class PermissionSkipAttribute : Attribute { }
```

Usage:
```csharp
[AllowAnonymous]
[PermissionSkip]
[HttpPost("login")]
public IActionResult Login([FromBody] LoginRequest request) { ... }
```

## Why two attributes? ([AllowAnonymous] + [PermissionSkip])

- `[AllowAnonymous]` — tells **ASP.NET Core's built-in JWT middleware** to allow unauthenticated requests through (no Bearer token required).
- `[PermissionSkip]` — tells the **custom `PermissionGlobalAttribute`** to skip the bit-flag permission check.

They guard two different layers:
1. The framework middleware rejects requests without a valid token unless `[AllowAnonymous]` is present.
2. The custom filter rejects requests without the required permission bit unless `[PermissionSkip]` is present.

The login endpoint needs both: it receives no token (it *issues* one), so it needs `[AllowAnonymous]` to get through the middleware, and `[PermissionSkip]` so the custom filter doesn't look for permission claims that don't exist yet.
