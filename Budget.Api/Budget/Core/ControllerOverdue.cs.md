# ControllerOverdue.cs

**Project:** Budget.Api
**Layer:** Controller (HTTP Entry Point)

## What it does

Returns a paginated list of overdue payments — bills that are past their due date and still `Pending`.

| Method | Route | Query Params | Description |
|--------|-------|-------------|-------------|
| GET | `api/overdue/payments` | `skip=0&take=20` | Paginated overdue payment list |

## Pagination pattern

```csharp
public async Task<IActionResult> GetOverdue(int skip = 0, int take = 20)
    => Ok(await _repo.SelectOverduePayments(skip, take));
```

SQL:
```sql
SELECT * FROM dbo.Payment
WHERE Status = 0           -- Pending
  AND DueAtUTC < @UtcNow
ORDER BY DueAtUTC ASC
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
```

`skip` and `take` are standard offset pagination. The frontend uses `skip=0&take=5` to show the top 5 most overdue on the dashboard widget, then the user can click "View All" to page through the rest.

## Dashboard widget

`DashboardComponent` calls this on load and shows the overdue count + top items as a quick heads-up. The payment status badge turns red for any overdue item.

## Direct repository access

Read-only, no business logic → controller calls `IRepositoryPassGenerate` (which has `SelectOverdueCount`) directly, consistent with the eMAR pattern for simple reads.
