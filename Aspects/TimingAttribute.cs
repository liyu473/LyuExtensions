using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Aspects;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LyuExtensions.Aspects;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class TimingAttribute : OverrideMethodAspect
{
    [IntroduceDependency]
    private readonly ILogger? _logger;

    /// <summary>
    /// 是否记录日志，默认为 true
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// 最后一次执行的耗时（毫秒）
    /// </summary>
    [Introduce]
    public long LastExecutionTime { get; private set; }

    public override dynamic? OverrideMethod()
    {
        var typeName = meta.Target.Type.ToDisplayString();
        var methodName = meta.Target.Method.Name;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = meta.Proceed();
            stopwatch.Stop();

            meta.This.LastExecutionTime = stopwatch.ElapsedMilliseconds;

            if (EnableLogging && _logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger.LogInformation("方法执行完成: {TypeName}.{MethodName}, 耗时: {ElapsedMilliseconds}ms",
                    typeName, methodName, stopwatch.ElapsedMilliseconds);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            meta.This.LastExecutionTime = stopwatch.ElapsedMilliseconds;

            if (EnableLogging && _logger != null)
            {
                _logger.LogError(ex, "方法执行异常: {TypeName}.{MethodName}, 耗时: {ElapsedMilliseconds}ms",
                    typeName, methodName, stopwatch.ElapsedMilliseconds);
            }

            throw;
        }
    }
}
