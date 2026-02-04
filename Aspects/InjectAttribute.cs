using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Diagnostics;

namespace LyuExtensions.Aspects;

/// <summary>
/// 自动注入依赖项的特性。
/// 在字段上标记此特性，会自动生成构造函数参数并赋值。
/// </summary>
/// <example>
/// <code>
/// public partial class MainWindow
/// {
///     [Inject] private readonly ILogger&lt;MainWindow&gt; _logger;
///     [Inject] private readonly MainViewModel _vm;
/// }
/// </code>
/// 编译后自动生成：
/// <code>
/// public MainWindow(ILogger&lt;MainWindow&gt; logger, MainViewModel vm)
/// {
///     _logger = logger;
///     _vm = vm;
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class InjectAttribute : FieldAspect
{
    public override void BuildAspect(IAspectBuilder<IField> builder)
    {
        var field = builder.Target;
        var declaringType = field.DeclaringType;

        // 获取参数名
        var parameterName = GetParameterName(field.Name);

        // 获取或创建构造函数
        var constructor = declaringType.Constructors
            .FirstOrDefault(c => !c.IsStatic);

        // 抑制 CS8618 警告（字段由构造函数注入，不会为 null）
        // 在字段级别抑制警告
        builder.Diagnostics.Suppress(
            new SuppressionDefinition("CS8618"),
            field);

        // 同时在声明类型级别抑制警告（针对构造函数）
        builder.With(declaringType).Diagnostics.Suppress(
            new SuppressionDefinition("CS8618"));

        if (constructor != null)
        {
            // 向现有构造函数添加参数
            var constructorBuilder = builder.With(constructor);

            constructorBuilder.IntroduceParameter(
                parameterName,
                field.Type,
                TypedConstant.Default(field.Type),
                pullStrategy: PullStrategy.IntroduceParameterAndPull(
                    parameterName,
                    field.Type,
                    TypedConstant.Default(field.Type)));

            constructorBuilder.AddInitializer(
                StatementFactory.Parse($"this.{field.Name} = {parameterName};"));
        }
        else
        {
            // 如果没有构造函数，引入一个新的
            var typeBuilder = builder.With(declaringType);
            var ctorResult = typeBuilder.IntroduceConstructor(
                nameof(ConstructorTemplate),
                buildConstructor: ctorBuilder =>
                {
                    ctorBuilder.AddParameter(parameterName, field.Type);
                },
                args: new { field, parameterName });
        }
    }

    [Template]
    private void ConstructorTemplate([CompileTime] IField _field, [CompileTime] string _parameterName)
    {
        // 模板参数通过 args 传递，实际赋值在 AddInitializer 中完成
    }

    /// <summary>
    /// 根据字段名生成参数名
    /// _logger -> logger
    /// _viewModel -> viewModel
    /// </summary>
    private static string GetParameterName(string fieldName)
    {
#pragma warning disable CA1866 // Metalama 编译时不支持 StartsWith(char)
        if (fieldName.StartsWith("_"))
#pragma warning restore CA1866
        {
            var name = fieldName[1..];
            if (name.Length > 0)
            {
                return char.ToLowerInvariant(name[0]) + name.Substring(1);
            }
            return name;
        }
        return char.ToLowerInvariant(fieldName[0]) + fieldName.Substring(1);
    }
}
