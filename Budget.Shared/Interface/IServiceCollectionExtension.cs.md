# IServiceCollectionExtension.cs (Budget.Shared)

**Project:** Budget.Shared
**Layer:** DI Registration

## What it does

Extension methods on `IServiceCollection` that register all repositories and services from the `Budget.Shared` project in one call.

```csharp
public static class IServiceCollectionExtension
{
    public static IServiceCollection AddSharedRepository(this IServiceCollection services)
    {
        services.AddScoped<IRepositoryAccount, RepositoryAccount>();
        services.AddScoped<IRepositoryCategory, RepositoryCategory>();
        services.AddScoped<IRepositoryBill, RepositoryBill>();
        services.AddScoped<IRepositorySetting, RepositorySetting>();
        services.AddScoped<IRepositoryUser, RepositoryUser>();
        services.AddScoped<RepositoryConfiguration>();
        return services;
    }

    public static IServiceCollection AddSharedService(this IServiceCollection services)
    {
        services.AddScoped<IServiceAccount, ServiceAccount>();
        services.AddScoped<IServiceCategory, ServiceCategory>();
        services.AddScoped<IServiceBill, ServiceBill>();
        return services;
    }
}
```

Called in `Program.cs`:
```csharp
builder.Services.AddSharedRepository();
builder.Services.AddSharedService();
```

## Why this pattern?

Without extension methods, `Program.cs` would need to list every registration from every project, growing to hundreds of lines and requiring `Program.cs` to know about implementation types it shouldn't care about.

With extension methods, each project owns its own registration. `Program.cs` just calls two methods and the entire DI graph is wired up.

This is the same pattern eMAR uses: `AddPassRepository()`, `AddPassService()`, `AddSharedRepository()`, etc.

## AddScoped vs AddSingleton vs AddTransient

- **`AddScoped`** — one instance per HTTP request. ✓ Used here because repositories open SqlConnections (short-lived, one per request is correct).
- **`AddSingleton`** — one instance for the app lifetime. Used for stateless caches or services with no request-level state.
- **`AddTransient`** — new instance every time it's injected. Rarely used; can cause multiple DB connections in one request.
