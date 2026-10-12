# ControllerAuthentication.cs

**Project:** Budget.Api
**Layer:** Controller (HTTP Entry Point)

## What it does

Handles login and returns a JWT token on success.

| Method | Route | Attributes |
|--------|-------|-----------|
| POST | `api/authentication/login` | `[AllowAnonymous]` `[PermissionSkip]` |

## The login flow

```csharp
1. Find user by username: _repoUser.SelectUserByUsername(request.Username)
2. If null → return 401 (don't reveal "user not found" vs "wrong password")
3. BCrypt.Verify(request.Password, user.PasswordHash)
4. If false → return 401
5. Load permissions: _repoUser.SelectPermissions(user.UserId)
6. Build JWT claims: sub=UserId, name=Username, perm=PermissionBits
7. Sign with HMAC-SHA256 using JwtOptions.Secret
8. Return { Token: "eyJ..." }
```

## JWT claims

The token carries these claims:

```
sub   → user.UserId (standard JWT subject claim)
name  → user.Username
perm  → permission bits as long string (custom claim)
exp   → expiry (current time + ExpiryMinutes)
```

`PermissionGlobalAttribute` reads the `perm` claim on every subsequent request.

## Security notes

- Both "user not found" and "wrong password" return the same `401 Unauthorized` response — this prevents **user enumeration** (attackers can't tell if a username exists).
- BCrypt comparison is deliberately slow (~100ms) — this is a feature, not a bug, making brute-force attacks impractical.
- The JWT secret must never be committed to source control; use environment variables or a secrets manager.

## Token storage (Angular side)

The login component stores the token in `localStorage`. The `SecurityInterceptor` reads it and adds `Authorization: Bearer <token>` to every HTTP request. See `login.component.ts.md` and `security.interceptor.ts.md`.
