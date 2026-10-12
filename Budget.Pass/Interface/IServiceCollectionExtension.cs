using Budget.Pass.Generate.Repository;
using Budget.Pass.Generate.Service;
using Budget.Pass.Submit.Repository;
using Budget.Pass.Submit.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Budget.Pass.Interface;

/// <summary>
/// DI registrations for the Pass project. Called from Program.cs.
/// Mirrors eMAR's medpass DI extension — each project owns its own registrations.
/// </summary>
public static class IServiceCollectionExtension
{
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
}
