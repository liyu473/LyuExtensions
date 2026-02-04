using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LyuExtensions.Aspects;

/// <summary>
/// 方法耗时记录特性。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class TimingAttribute : OverrideMethodAspect
{
    [IntroduceDependency]
    private readonly ILogger? _logger;

    /// <summary>
    /// 日志记录级别，默认为 Information (2)
    /// 0=Trace, 1=Debug, 2=Information, 3=Warning, 4=Error, 5=Critical, 6=None
    /// </summary>
    public int LogLevelValue { get; set; } = 2; // Information

    /// <summary>
    /// 同步方法的耗时记录
    /// </summary>
    public override dynamic? OverrideMethod()
    {
        var typeName = meta.Target.Type.ToDisplayString();
        var methodName = meta.Target.Method.Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = meta.Proceed();
            stopwatch.Stop();

            LogCompletion(typeName, methodName, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogException(ex, typeName, methodName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// 异步方法的耗时记录（等待 Task 完成后再计算耗时）
    /// </summary>
    public override async Task<dynamic?> OverrideAsyncMethod()
    {
        var typeName = meta.Target.Type.ToDisplayString();
        var methodName = meta.Target.Method.Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await meta.ProceedAsync();
            stopwatch.Stop();

            LogCompletion(typeName, methodName, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogException(ex, typeName, methodName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    [Template]
    private void LogCompletion([CompileTime] string typeName, [CompileTime] string methodName, long elapsedMs)
    {
        var logLevel = (LogLevel)LogLevelValue;
        if (_logger?.IsEnabled(logLevel) == true)
        {
            _logger.Log(logLevel, "方法执行完成: {TypeName}.{MethodName}, 耗时: {ElapsedMilliseconds}ms",
                typeName, methodName, elapsedMs);
        }
    }

    [Template]
    private void LogException(Exception ex, [CompileTime] string typeName, [CompileTime] string methodName, long elapsedMs)
    {
        if (_logger != null)
        {
            _logger.LogError(ex, "方法执行异常: {TypeName}.{MethodName}, 耗时: {ElapsedMilliseconds}ms",
                typeName, methodName, elapsedMs);
        }
    }
}
