# IServiceCollectionExtension.cs (Budget.Pass)

**Project:** Budget.Pass
**Layer:** DI Registration

## What it does

Extension methods that register all repositories and services from `Budget.Pass`.

```csharp
public static IServiceCollection AddPassRepository(this IServiceCollection services)
{
    services.AddScoped<IRepositoryPassGenerate, RepositoryPassGenerate>();
    services.AddScoped<IRepositoryPassSubmit, RepositoryPassSubmit>();
    return services;
}

public static IServiceCollection AddPassService(this IServiceCollection services)
{
    services.AddScoped<IServicePassGenerate, ServicePassGenerate>();
    services.AddScoped<IServicePassSubmit, ServicePassSubmit>();
    return services;
}
```

Called in `Program.cs`:
```csharp
builder.Services.AddPassRepository();
builder.Services.AddPassService();
```

## Why a separate extension file per project?

Each project registers its own dependencies. `Budget.Api`'s `Program.cs` doesn't need to know that `ServicePassGenerate` depends on `IRepositoryPassGenerate` — it just calls `AddPassService()`. The pass project takes responsibility for its own wiring.

This is the same pattern as `Budget.Shared`'s `IServiceCollectionExtension.cs`. In eMAR, every bounded context (pass, billing, pharmacy) has its own `AddXxxRepository` / `AddXxxService` pair.
