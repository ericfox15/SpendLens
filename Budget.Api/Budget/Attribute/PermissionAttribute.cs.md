# PermissionAttribute.cs

**Project:** Budget.Api
**Layer:** Attribute / Authorization

## What it does

A custom attribute that marks a controller action with the minimum permission required to call it.

```csharp
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class PermissionAttribute : Attribute
{
    public EPermission Required { get; }
    public PermissionAttribute(EPermission required) => Required = required;
}
```

Usage:
```csharp
[Permission(EPermission.Write)]
[HttpPost]
public IActionResult Create([FromBody] FAccount account) { ... }
```

## How it's enforced

`PermissionGlobalAttribute` (a global action filter) runs on every request:
1. Checks if the action has `[PermissionSkip]` → skip check
2. Checks if the action has `[Permission]` → extract required bit
3. Reads the JWT claim `"perm"` → user's permission bits (long)
4. If `(userBits & required) == 0` → return 403 Forbidden

## Why not use ASP.NET Core's built-in [Authorize(Policy="...")]?

The built-in policy system works but requires policy registration in `Program.cs`. The custom attribute keeps permission declarations close to the action (readable at a glance) and uses the same bit-flag EPermission enum the rest of the app knows. eMAR uses this same pattern, so mirroring it here teaches you the real production approach.
