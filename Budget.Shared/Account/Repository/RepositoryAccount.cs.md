# RepositoryAccount.cs

**Project:** Budget.Shared
**Layer:** Repository (Data Access)

## What it does

Implements `IRepositoryAccount` — all SQL for the `dbo.Account` table using Dapper.

| Method | SQL |
|--------|-----|
| `SelectAll` | `SELECT ... FROM dbo.Account WHERE IsActive = 1` |
| `SelectById` | `SELECT ... WHERE AccountId = @Id AND IsActive = 1` |
| `Insert` | `INSERT INTO dbo.Account ... ; SELECT SCOPE_IDENTITY()` |
| `Update` | `UPDATE dbo.Account SET ... WHERE AccountId = @Id` |
| `Deactivate` | `UPDATE dbo.Account SET IsActive = 0 WHERE AccountId = @Id` |

## Dapper key concepts

**Parameter binding** — Dapper maps `new { Id = id }` to `@Id` in the SQL string. No SQL injection risk because parameters are sent separately from the query text.

**`SCOPE_IDENTITY()`** — after `INSERT`, this returns the newly generated identity value as the new `AccountId`. The method returns it as `int` for the caller to store.

**`QuerySingleOrDefaultAsync<T>`** — returns one row mapped to `T`, or `default(T)` if nothing matches. Use this when you expect 0 or 1 results.

**`QueryAsync<T>`** — returns all matching rows as `IEnumerable<T>`.

**`ExecuteAsync`** — runs a statement that doesn't return rows (UPDATE, DELETE).

## Why inline SQL (not stored procs)?

The convention here is **inline SQL for simple CRUD** and **stored procedures for complex logic**. Account operations are pure CRUD so they use inline SQL. The pass generation logic (`usp_GeneratePass`) uses a stored proc because it involves joins, conditional logic, and multiple tables.
