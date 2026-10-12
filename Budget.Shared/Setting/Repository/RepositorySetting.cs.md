# RepositorySetting.cs (and IRepositorySetting.cs)

**Project:** Budget.Shared
**Layer:** Repository (Data Access)

## What it does

Manages the `dbo.Setting` key-value table. The key operation is an **upsert** using SQL MERGE.

```sql
MERGE dbo.Setting AS target
USING (SELECT @Path AS Path) AS source ON target.Path = source.Path
WHEN MATCHED THEN UPDATE SET Value = @Value
WHEN NOT MATCHED THEN INSERT (Path, Value) VALUES (@Path, @Value);
```

## Why MERGE for upsert?

The alternative is:
1. SELECT to check if the row exists
2. If yes → UPDATE; if no → INSERT

The problem: between step 1 and step 2, another request could INSERT, causing a duplicate key error. MERGE is atomic — the check and the write happen in one statement, eliminating the race condition.

## Operations

| Method | SQL |
|--------|-----|
| `SelectAll()` | `SELECT Path, Value FROM dbo.Setting` |
| `SelectByPath(path)` | `SELECT Value WHERE Path = @Path` |
| `Upsert(path, value)` | MERGE statement above |

## RepositoryConfiguration wraps this

`RepositoryConfiguration` is not a repository itself — it's a **typed settings accessor** that calls `RepositorySetting.SelectByPath()` and parses the string into the right type. Read `RepositoryConfiguration.cs.md` for the typed getters.
