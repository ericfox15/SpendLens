# ControllerAccount.cs

**Project:** Budget.Api
**Layer:** Controller (HTTP Entry Point)

## What it does

Exposes CRUD operations for accounts over HTTP. Maps to `api/account`.

| Method | Route | Permission | Description |
|--------|-------|-----------|-------------|
| GET | `api/account` | Read | Get all active accounts |
| GET | `api/account/{id}` | Read | Get account by ID |
| POST | `api/account` | Write | Create new account |
| PUT | `api/account/{id}` | Write | Update account |
| DELETE | `api/account/{id}` | Write | Soft-delete (deactivate) |

## Simple reads call repository directly

Notice that GET endpoints in this controller inject `IRepositoryAccount` and call it directly, bypassing `IServiceAccount`:

```csharp
// In the controller constructor:
private readonly IRepositoryAccount _repo;
private readonly IServiceAccount _service;

// GET endpoints use _repo directly:
[HttpGet] public async Task<IActionResult> GetAll() => Ok(await _repo.SelectAll());

// Write endpoints go through _service:
[HttpPost] public async Task<IActionResult> Create([FromBody] FAccount a) => Ok(await _service.Create(a));
```

This is a deliberate eMAR pattern for simple reads: when there's no business logic to apply, going directly to the repository is simpler and slightly faster. The service layer is reserved for operations with real logic (validation, cross-entity updates, etc.).

## Response types

- `200 OK` with data — successful reads and updates
- `201 Created` with the new ID — successful creates (could also use `CreatedAtAction`)
- `204 No Content` — successful deletes

The `[ApiController]` attribute on the base class handles `400 Bad Request` automatically for model validation failures.
