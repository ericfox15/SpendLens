# EPermission.cs

**Project:** Budget.Application.Contracts
**Layer:** Contracts

## What it does

Bit-flag enum that controls what a user is allowed to do in the app.

```
Read  = 1   → can view data (GET endpoints)
Write = 2   → can create/update/delete (POST, PUT, DELETE endpoints)
Admin = 4   → can manage users and settings
```

Because it's a bit-flag (`[Flags]`), you can combine permissions: `Read | Write = 3`. A single integer stored in `dbo.Permission` represents multiple grants — no join table needed.

## How it's used

1. **`[Permission(EPermission.Write)]`** attribute goes on a controller action.
2. **`PermissionGlobalAttribute`** (the global filter) reads this attribute at runtime, checks the JWT claims, and returns 403 if the user doesn't have the required bit.
3. **`[PermissionSkip]`** bypasses this check entirely (used on the login endpoint).

## Bit-flag math reminder

```csharp
long userBits = 3;  // Read | Write
bool canWrite = (userBits & (long)EPermission.Write) != 0;  // true
bool isAdmin  = (userBits & (long)EPermission.Admin)  != 0;  // false
```

The backing type is `long` (64-bit) — overkill for three values, but matches eMAR's pattern where permission sets can grow.

## eMAR parallel

eMAR uses the same bit-flag permission pattern stored per-user in the database, checked in a global filter. This is the exact same approach here, deliberately teaching that pattern.
