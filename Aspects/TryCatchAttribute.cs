using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Aspects;
using Microsoft.Extensions.Logging;

namespace LyuExtensions.Aspects;

/// <summary>
/// 自动为方法添加 try-catch 异常处理的特性。
/// 默认行为：捕获异常、记录日志、吞掉异常并返回默认值。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class TryCatchAttribute : OverrideMethodAspect
{
    [IntroduceDependency]
    private readonly ILogger? _exceptionLogger;

    /// <summary>
    /// 是否记录异常日志，默认为 true
    /// </summary>
    public bool LogException { get; set; } = true;

    /// <summary>
    /// 异常发生时的默认返回值
    /// 对于引用类型默认为 null，对于值类型默认为 default(T)
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// 同步方法的 try-catch 包装
    /// </summary>
    public override dynamic? OverrideMethod()
    {
        var typeName = meta.Target.Type.ToDisplayString();
        var methodName = meta.Target.Method.Name;

        try
        {
            return meta.Proceed();
        }
        catch (Exception ex)
        {
            HandleException(ex, typeName, methodName);
            return DefaultValue;
        }
    }

    /// <summary>
    /// 异步方法的 try-catch 包装
    /// </summary>
    public override async Task<dynamic?> OverrideAsyncMethod()
    {
        var typeName = meta.Target.Type.ToDisplayString();
        var methodName = meta.Target.Method.Name;

        try
        {
            return await meta.ProceedAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex, typeName, methodName);
            return DefaultValue;
        }
    }

    [Template]
    private void HandleException(Exception ex, [CompileTime] string typeName, [CompileTime] string methodName)
    {
        if (LogException && _exceptionLogger != null)
        {
            _exceptionLogger.LogError(ex, "[TryCatch] 捕获异常: {TypeName}.{MethodName}, 异常类型: {ExceptionType}, 消息: {Message}",
                typeName, methodName, ex.GetType().Name, ex.Message);
        }
    }
}
