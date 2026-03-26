using LyuExtensions.Aspects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace LyuExtensions.Extensions;

/// <summary>
/// Scans assemblies and registers services marked by attributes.
/// </summary>
public static class ServiceRegistrar
{
    /// <summary>
    /// Registers attributed services from target assemblies.
    /// </summary>
    public static IServiceCollection RegisterServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract);

            foreach (var type in types)
            {
                RegisterServiceAttribute(services, type);
                RegisterHostedServiceAttribute(services, type);
            }
        }

        return services;
    }

    private static void RegisterServiceAttribute(IServiceCollection services, Type type)
    {
        var serviceAttr = type.GetCustomAttribute<ServiceAttribute>();
        if (serviceAttr == null)
        {
            return;
        }

        var serviceType = serviceAttr.ServiceType ?? type;

        if (serviceAttr.ServiceKey != null)
        {
            switch (serviceAttr.Lifetime)
            {
                case ServiceLifetimeType.Singleton:
                    services.AddKeyedSingleton(serviceType, serviceAttr.ServiceKey, type);
                    break;
                case ServiceLifetimeType.Scoped:
                    services.AddKeyedScoped(serviceType, serviceAttr.ServiceKey, type);
                    break;
                case ServiceLifetimeType.Transient:
                    services.AddKeyedTransient(serviceType, serviceAttr.ServiceKey, type);
                    break;
            }
        }
        else
        {
            switch (serviceAttr.Lifetime)
            {
                case ServiceLifetimeType.Singleton:
                    services.AddSingleton(serviceType, type);
                    break;
                case ServiceLifetimeType.Scoped:
                    services.AddScoped(serviceType, type);
                    break;
                case ServiceLifetimeType.Transient:
                    services.AddTransient(serviceType, type);
                    break;
            }
        }
    }

    private static void RegisterHostedServiceAttribute(IServiceCollection services, Type type)
    {
        var hostedServiceAttr = type.GetCustomAttribute<HostedServiceAttribute>();
        if (hostedServiceAttr == null || !typeof(IHostedService).IsAssignableFrom(type))
        {
            return;
        }

        // Make hosted service injectable by concrete type.
        services.TryAddSingleton(type);

        // Ensure host executes the same singleton instance.
        services.AddSingleton(
            typeof(IHostedService),
            provider => (IHostedService)provider.GetRequiredService(type));
    }
}
