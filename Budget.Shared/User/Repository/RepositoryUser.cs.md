# RepositoryUser.cs (and IRepositoryUser.cs)

**Project:** Budget.Shared
**Layer:** Repository (Data Access)

## What it does

Data access for `dbo.User` and `dbo.Permission`. Handles the auth-specific queries.

| Method | Purpose |
|--------|---------|
| `SelectUserByUsername(username)` | Returns `FUser` (with hash) for login verification |
| `SelectPermissions(userId)` | Returns `SUserPermission` with the bit-flag permission value |
| `InsertUser(user)` | Creates a new user (admin registration) |
| `InsertPermission(userId, permission)` | Grants a permission bit to a user |

## Why separate tables for User and Permission?

A single `Users` table with a `Permission` column would work, but the eMAR pattern separates them so permissions can be granted and revoked individually without updating the user row. It also opens the door to per-facility permissions in multi-tenant contexts.

## SelectUserByUsername — username lookup

```sql
SELECT UserId, Username, PasswordHash
FROM dbo.User
WHERE Username = @Username
```

Returns `null` if no match. `ControllerAuthentication` checks for null and returns 401 before attempting BCrypt verification. **Never** return a different error for "wrong password" vs "user not found" to prevent user enumeration attacks.

## SelectPermissions

```sql
SELECT u.UserId, u.Username, p.Permission
FROM dbo.User u
JOIN dbo.Permission p ON u.UserId = p.UserId
WHERE u.UserId = @UserId
```

Returns as `SUserPermission` (the S* read model). The `Permission` long value goes into the JWT as a claim.
