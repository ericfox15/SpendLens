using Budget.Shared.Account.Repository;
using Budget.Shared.Account.Service;
using Budget.Shared.Bill.Repository;
using Budget.Shared.Bill.Service;
using Budget.Shared.Category.Repository;
using Budget.Shared.Category.Service;
using Budget.Shared.Setting.Repository;
using Budget.Shared.Setting.Service;
using Budget.Shared.User.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Budget.Shared.Interface;

/// <summary>
/// Registers all Shared project repositories and services into the DI container.
/// Called from Program.cs in Budget.Api. Mirrors eMAR's extension-method approach so
/// each project owns its own registrations — the host just calls AddSharedRepository() etc.
/// </summary>
public static class IServiceCollectionExtension
{
    public static IServiceCollection AddSharedRepository(this IServiceCollection services)
    {
        services.AddScoped<IRepositoryAccount, RepositoryAccount>();
        services.AddScoped<IRepositoryCategory, RepositoryCategory>();
        services.AddScoped<IRepositoryBill, RepositoryBill>();
        services.AddScoped<IRepositorySetting, RepositorySetting>();
        services.AddScoped<IRepositoryUser, RepositoryUser>();
        return services;
    }

    public static IServiceCollection AddSharedService(this IServiceCollection services)
    {
        services.AddScoped<IServiceAccount, ServiceAccount>();
        services.AddScoped<IServiceCategory, ServiceCategory>();
        services.AddScoped<IServiceBill, ServiceBill>();
        services.AddScoped<RepositoryConfiguration>();
        return services;
    }
}
