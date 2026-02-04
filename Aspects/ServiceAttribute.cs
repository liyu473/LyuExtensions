namespace LyuExtensions.Aspects;

/// <summary>
/// 服务生命周期枚举
/// </summary>
public enum ServiceLifetimeType
{
    Singleton,
    Scoped,
    Transient
}

/// <summary>
/// 自动注册服务到 DI 容器的基础特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ServiceAttribute : Attribute
{
    /// <summary>
    /// 服务生命周期
    /// </summary>
    public ServiceLifetimeType Lifetime { get; }

    /// <summary>
    /// 服务接口类型，如果为 null 则注册为自身类型
    /// </summary>
    public Type? ServiceType { get; set; }

    /// <summary>
    /// 服务键（用于 Keyed Services，.NET 8+）
    /// </summary>
    public object? ServiceKey { get; set; }

    public ServiceAttribute(ServiceLifetimeType lifetime = ServiceLifetimeType.Scoped)
    {
        Lifetime = lifetime;
    }
}

/// <summary>
/// 注册为单例服务
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class SingletonAttribute : ServiceAttribute
{
    public SingletonAttribute() : base(ServiceLifetimeType.Singleton)
    {
    }
}

/// <summary>
/// 注册为作用域服务
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ScopedAttribute : ServiceAttribute
{
    public ScopedAttribute() : base(ServiceLifetimeType.Scoped)
    {
    }
}

/// <summary>
/// 注册为瞬态服务
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class TransientAttribute : ServiceAttribute
{
    public TransientAttribute() : base(ServiceLifetimeType.Transient)
    {
    }
}

/// <summary>
/// 注册为 HostedService（后台服务）
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class HostedServiceAttribute : Attribute
{
}
