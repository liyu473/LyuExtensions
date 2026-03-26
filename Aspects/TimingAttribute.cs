using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LyuExtensions.Aspects;

/// <summary>
/// Method execution time logging aspect.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class TimingAttribute : OverrideMethodAspect
{
    [CompileTime]
    private IFieldOrProperty? _loggerMember;

    /// <summary>
    /// Log level value.
    /// 0=Trace, 1=Debug, 2=Information, 3=Warning, 4=Error, 5=Critical, 6=None.
    /// </summary>
    public int LogLevelValue { get; set; } = 2;

    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        _loggerMember = FindExistingLoggerMember(builder.Target.DeclaringType);

        if (_loggerMember == null)
        {
            var dependencyResult = builder.With(builder.Target.DeclaringType).IntroduceDependency(
                typeof(ILogger),
                new DependencyOptions
                {
                    MemberName = "_logger",
                    MemberKind = DeclarationKind.Field
                });

            if (dependencyResult.Outcome != AdviceOutcome.Error)
            {
                _loggerMember = dependencyResult.Declaration;
            }
        }

        base.BuildAspect(builder);
    }

    [CompileTime]
    private static IFieldOrProperty? FindExistingLoggerMember(INamedType targetType)
    {
        return targetType.FieldsAndProperties
            .Where(member => !member.IsStatic)
            .FirstOrDefault(member => IsLoggerType(member.Type));
    }

    [CompileTime]
    private static bool IsLoggerType(IType type)
    {
        var typeName = type.ToDisplayString();
        return typeName.Contains("Microsoft.Extensions.Logging.ILogger");
    }

    public override dynamic? OverrideMethod()
    {
        var typeName = meta.Target.Type.ToDisplayString();
        var methodName = meta.Target.Method.Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = meta.Proceed();
            stopwatch.Stop();

            LogCompletion(_loggerMember, typeName, methodName, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogException(_loggerMember, ex, typeName, methodName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public override async Task<dynamic?> OverrideAsyncMethod()
    {
        var typeName = meta.Target.Type.ToDisplayString();
        var methodName = meta.Target.Method.Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await meta.ProceedAsync();
            stopwatch.Stop();

            LogCompletion(_loggerMember, typeName, methodName, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogException(_loggerMember, ex, typeName, methodName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    [Template]
    private void LogCompletion([CompileTime] IFieldOrProperty? loggerMember, [CompileTime] string typeName, [CompileTime] string methodName, long elapsedMs)
    {
        if (loggerMember == null)
        {
            return;
        }

        var logger = (ILogger?)loggerMember.Value;
        var logLevel = (LogLevel)LogLevelValue;

        if (logger?.IsEnabled(logLevel) == true)
        {
            logger.Log(
                logLevel,
                "方法执行完成: {TypeName}.{MethodName}, 耗时: {ElapsedMilliseconds}ms",
                typeName,
                methodName,
                elapsedMs);
        }
    }

    [Template]
    private void LogException([CompileTime] IFieldOrProperty? loggerMember, Exception ex, [CompileTime] string typeName, [CompileTime] string methodName, long elapsedMs)
    {
        if (loggerMember == null)
        {
            return;
        }

        var logger = (ILogger?)loggerMember.Value;
        if (logger != null)
        {
            logger.LogError(
                ex,
                "方法执行异常: {TypeName}.{MethodName}, 耗时: {ElapsedMilliseconds}ms",
                typeName,
                methodName,
                elapsedMs);
        }
    }
}
