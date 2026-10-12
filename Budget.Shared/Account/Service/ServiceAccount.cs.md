# ServiceAccount.cs (and IServiceAccount.cs)

**Project:** Budget.Shared
**Layer:** Service

## What it does

Business logic layer between the controller and the repository. For accounts, it mostly delegates to `IRepositoryAccount` since account management is simple CRUD with no complex rules.

```csharp
public class ServiceAccount : IServiceAccount
{
    private readonly IRepositoryAccount _repo;
    public ServiceAccount(IRepositoryAccount repo) => _repo = repo;

    public Task<IEnumerable<FAccount>> GetAll() => _repo.SelectAll();
    public Task<FAccount> GetById(int id) => _repo.SelectById(id);
    public Task<int> Create(FAccount account) => _repo.Insert(account);
    public Task Update(int id, FAccount account) => _repo.Update(id, account);
    public Task Remove(int id) => _repo.Deactivate(id);
}
```

## Why have a service at all if it just delegates?

Even when business logic is absent now, the service layer exists for:

1. **Future logic** — validation, cross-entity checks, event emission can be added here without changing the controller or repository.
2. **Testability** — controllers are tested by mocking `IServiceAccount`; services are tested by mocking `IRepositoryAccount`. Each layer is independently testable.
3. **Consistency** — the entire app follows Controller → Service → Repository. Breaking this pattern for "simple" entities would make the architecture inconsistent.

In eMAR, simple entities like "location" or "shift" follow this same thin-service pattern.

## IServiceAccount.cs

Defines the interface injected into the controller. Same reasoning as `IRepositoryAccount` — enables DI swapping and clean unit testing.
