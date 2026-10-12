# RepositoryBill.cs (and IRepositoryBill.cs)

**Project:** Budget.Shared
**Layer:** Repository (Data Access)

## What it does

All SQL for `dbo.Bill`, including JOIN queries that populate display-name fields.

## Notable: the JOIN SELECT

`SelectAll()` and `SelectById()` join `dbo.Account` and `dbo.Category` so `FBill.AccountName` and `FBill.CategoryName` are populated in a single query:

```sql
SELECT b.*, a.Name AS AccountName, c.Name AS CategoryName
FROM dbo.Bill b
JOIN dbo.Account a ON b.AccountId = a.AccountId
JOIN dbo.Category c ON b.CategoryId = c.CategoryId
WHERE b.IsActive = 1
```

Dapper automatically maps the `AccountName` and `CategoryName` columns from the result into the matching properties on `FBill` — no extra mapping code needed.

## AnchorDateUTC persistence

`DateTimeOffset` maps cleanly to SQL Server's `datetimeoffset` column type via Dapper's built-in type handling. Always store dates as UTC (`DateTimeOffset.UtcNow`) to avoid timezone conversion bugs.

## Insert returns the new BillId

```csharp
return await conn.QuerySingleAsync<int>(
    "INSERT INTO dbo.Bill (...) VALUES (...); SELECT SCOPE_IDENTITY();",
    bill
);
```

The controller uses this returned ID if it needs to do something with the new entity immediately.
