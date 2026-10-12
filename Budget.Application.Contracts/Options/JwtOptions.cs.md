# JwtOptions.cs

**Project:** Budget.Application.Contracts
**Layer:** Contracts / Configuration

## What it does

Holds the configuration for JWT (JSON Web Token) authentication:

| Property | Default | Purpose |
|----------|---------|---------|
| `Secret` | — | HMAC-SHA256 signing key (keep this long and random) |
| `Issuer` | — | Who minted the token (your API URL) |
| `Audience` | — | Who the token is for (your frontend URL) |
| `ExpiryMinutes` | 480 (8 hours) | How long until a token expires |

Bound from `appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "some-very-long-random-secret",
    "Issuer": "https://localhost:5001",
    "Audience": "budget-frontend",
    "ExpiryMinutes": 480
  }
}
```

## How JWTs work (quick primer)

1. User POSTs username + password to `api/authentication/login`.
2. `ControllerAuthentication` verifies the password with BCrypt.
3. If valid, it creates a JWT signed with `Secret` that contains the user's ID and permission bits as **claims**.
4. Every subsequent request sends this token in `Authorization: Bearer <token>`.
5. ASP.NET Core's middleware validates the signature and expiry automatically.
6. `PermissionGlobalAttribute` reads the claims from the validated token to check permissions.

## Security note

`Secret` should be at least 32 characters (256 bits) for HMAC-SHA256. In production, store it in an environment variable or secrets manager — **never commit it to source control**.
