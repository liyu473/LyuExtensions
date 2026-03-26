using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Microsoft.Extensions.Logging;

namespace LyuExtensions.Aspects;

/// <summary>
/// Adds try-catch handling to a method and optionally logs exceptions.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class TryCatchAttribute : OverrideMethodAspect
{
    [CompileTime]
    private IFieldOrProperty? _loggerMember;

    /// <summary>
    /// Whether exception logs should be written.
    /// </summary>
    public bool LogException { get; set; } = true;

    /// <summary>
    /// Default return value when exception is caught.
    /// </summary>
    public object? DefaultValue { get; set; }

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

        try
        {
            return meta.Proceed();
        }
        catch (Exception ex)
        {
            HandleException(_loggerMember, ex, typeName, methodName);
            return DefaultValue;
        }
    }

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
            HandleException(_loggerMember, ex, typeName, methodName);
            return DefaultValue;
        }
    }

    [Template]
    private void HandleException([CompileTime] IFieldOrProperty? loggerMember, Exception ex, [CompileTime] string typeName, [CompileTime] string methodName)
    {
        if (!LogException || loggerMember == null)
        {
            return;
        }

        var logger = (ILogger?)loggerMember.Value;
        if (logger != null)
        {
            logger.LogError(
                ex,
                "[TryCatch] 捕获异常: {TypeName}.{MethodName}, 异常类型: {ExceptionType}, 消息: {Message}",
                typeName,
                methodName,
                ex.GetType().Name,
                ex.Message);
        }
    }
}
