# IRepositoryAccount.cs

**Project:** Budget.Shared
**Layer:** Repository Interface

## What it does

Defines the contract for account data access. `RepositoryAccount.cs` is the implementation; this interface is what gets registered in DI and what the service layer injects.

```csharp
public interface IRepositoryAccount
{
    Task<IEnumerable<FAccount>> SelectAll();
    Task<FAccount> SelectById(int id);
    Task<int> Insert(FAccount account);
    Task Update(int id, FAccount account);
    Task Deactivate(int id);  // soft-delete
}
```

## Why interfaces for repositories?

1. **Testability** — unit tests swap in a mock (`Mock<IRepositoryAccount>`) without hitting the database.
2. **DI registration** — `services.AddScoped<IRepositoryAccount, RepositoryAccount>()` lets you swap implementations without changing call sites.
3. **Contracts** — the interface says what data operations exist; the implementation says how SQL gets it done. These concerns stay separate.

In eMAR, every repository follows this same interface + implementation split. It's one of the core patterns to internalize.

## Interface vs. abstract class

Repositories use **interfaces** (not abstract classes) because they have no shared behavior — each repository talks to its own table(s). Shared behavior (getting a connection) lives in `FRepositoryBase`, which is an abstract class the implementations inherit from.
