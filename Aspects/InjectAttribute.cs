using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace LyuExtensions.Aspects;

/// <summary>
/// 自动注入依赖项的特性。
/// 在字段或属性上标记此特性,会自动实现依赖注入。
/// 继承 Metalama.Extensions.DependencyInjection.DependencyAttribute
///
/// </summary>
/// <example>
/// <code>
/// public partial class MainWindow
/// {
///     [Inject] private readonly ILogger&lt;MainWindow&gt; _logger = default!;
///     [Inject] private readonly MainViewModel _vm = default!;
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
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class InjectAttribute : DependencyAttribute
{
    private static readonly SuppressionDefinition SuppressCs8618 = new("CS8618");

    public override void BuildAspect(IAspectBuilder<IFieldOrProperty> builder)
    {
        // 抑制 CS8618 警告
        builder.Diagnostics.Suppress(SuppressCs8618, builder.Target);

        // 在类型级别也抑制
        builder.With(builder.Target.DeclaringType).Diagnostics.Suppress(SuppressCs8618);

        base.BuildAspect(builder);
    }
}
