# SUserPermission.cs

**Project:** Budget.Shared
**Layer:** Read Model (Summary)

## What it does

A **read-only projection** that joins `dbo.User` with `dbo.Permission` to get what a user can do. Used during authentication to populate JWT claims.

```
UserId     → matches dbo.User.UserId
Username   → from dbo.User
Permission → bit-flag long from dbo.Permission
```

## S* naming convention

`S` prefix = **Summary model** in eMAR convention. These are flattened, read-optimized projections — often the result of a SQL JOIN — that don't map to a single table. You don't INSERT/UPDATE via an S* model; you only SELECT into it.

Contrast with `F*` models (full domain objects used for CRUD).

## How it's used in the auth flow

```
LoginRequest
    → RepositoryUser.SelectUserByUsername(username)  → FUser (has PasswordHash)
    → BCrypt.Verify(password, hash)
    → RepositoryUser.SelectPermissions(userId)       → SUserPermission (has Permission bits)
    → build JWT with claims: sub=userId, perm=permissionBits
```

The `SUserPermission.Permission` long value gets embedded as a JWT claim. `PermissionGlobalAttribute` reads it back on every request and checks the required bit.
