# RepositoryCategory.cs (and IRepositoryCategory.cs)

**Project:** Budget.Shared
**Layer:** Repository (Data Access)

## What it does

Standard Dapper CRUD for `dbo.Category`. Same shape as `RepositoryAccount` — select all, select by ID, insert, update, soft-delete.

## MonthlyLimit handling

`MonthlyLimit` is `decimal?` (nullable). When a category has no limit, the value is `NULL` in SQL. Dapper maps SQL `NULL` to C# `null` for nullable types automatically — no special handling needed.

In `ControllerReport`, if `MonthlyLimit` is null, the "over budget" warning is simply skipped for that category.

## Category soft-delete note

Categories can't be deactivated if they have active bills referencing them — this constraint is enforced at the application layer in `ServiceCategory` before calling `Deactivate()`. The SQL schema also has FK relationships, so the DB would reject it too, but it's better to return a user-friendly error from the service than a raw SQL exception.
