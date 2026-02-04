using LyuExtensions.Aspects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace LyuExtensions.Extensions;

/// <summary>
/// 服务注册器，用于扫描并注册带有 Service 特性的类
/// </summary>
public static class ServiceRegistrar
{
    /// <summary>
    /// 扫描并注册带有 Service 特性的类
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="assemblies">要扫描的程序集，如果为空则扫描调用程序集</param>
    /// <returns></returns>
    public static IServiceCollection RegisterServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var type in types)
            {
                // 处理 ServiceAttribute 及其派生类
                var serviceAttr = type.GetCustomAttribute<ServiceAttribute>();
                if (serviceAttr != null)
                {
                    var serviceType = serviceAttr.ServiceType ?? type;
                    
                    // 如果指定了 ServiceKey，使用 Keyed Services
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
                    continue;
                }

                // 处理 HostedServiceAttribute
                var hostedServiceAttr = type.GetCustomAttribute<HostedServiceAttribute>();
                if (hostedServiceAttr != null)
                {
                    if (typeof(IHostedService).IsAssignableFrom(type))
                    {
                        services.AddHostedService(provider => 
                            (IHostedService)ActivatorUtilities.CreateInstance(provider, type));
                    }
                }
            }
        }

        return services;
    }
}
