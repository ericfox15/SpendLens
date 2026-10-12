# FUser.cs

**Project:** Budget.Shared
**Layer:** Domain Model

## What it does

Represents an application user. Maps to `dbo.User`.

| Property | Notes |
|----------|-------|
| `UserId` | Identity PK |
| `Username` | Login name (unique, case-insensitive in practice) |
| `PasswordHash` | BCrypt hash of the password — **never the plain text password** |

## BCrypt password hashing

The app uses `BCrypt.Net-Next` for password storage:

```csharp
// On registration / seed:
string hash = BCrypt.Net.BCrypt.HashPassword("mypassword");  // includes salt

// On login verification:
bool valid = BCrypt.Net.BCrypt.Verify("mypassword", storedHash);
```

BCrypt is a **slow** hashing algorithm by design — it's computationally expensive to brute-force. Each hash includes an embedded salt so two identical passwords produce different hashes, preventing rainbow table attacks.

**Never** store plain-text passwords or use fast hashes like MD5/SHA1 for passwords.

## What's NOT on this model

Permission data is in a separate `dbo.Permission` table (one row per user per permission grant). The `SUserPermission` read model joins `User` + `Permission` together for the auth check.

## Seed user

`0001_BUDGET_Init.sql` seeds a default admin user. In a real deployment you'd immediately change the password or use a migration that only seeds if no users exist.
