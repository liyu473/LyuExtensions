using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Extensions;

/// <summary>
/// 常见于Wpf Mvvm绑定
/// </summary>
public static class ObjectExtension
{
    /// <summary>
    /// 将 <paramref name="source"/> 对象的可读写属性复制到 <paramref name="target"/>，保持目标实例不变。
    /// </summary>
    /// <typeparam name="T">引用类型的对象。</typeparam>
    /// <param name="target">作为复制目标的对象，不能为空。</param>
    /// <param name="source">提供属性值的对象，不能为空。</param>
    /// <exception cref="ArgumentNullException">
    /// 当 <paramref name="target"/> 或 <paramref name="source"/> 为 <c>null</c> 时抛出。
    /// </exception>
    public static void UpdatePropertiesFrom<T>(this T target, T source)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);

        if (ReferenceEquals(target, source))
        {
            return;
        }

        foreach (var property in PropertyCache<T>.WritableProperties)
        {
            var value = property.GetValue(source);
            property.SetValue(target, value);
        }
    }

    /// <summary>
    /// 高性能复制：利用表达式树缓存，将 <paramref name="source"/> 的属性快速应用到 <paramref name="target"/>。
    /// </summary>
    /// <typeparam name="T">引用类型的对象。</typeparam>
    /// <param name="target">复制目标实例，不能为空。</param>
    /// <param name="source">提供属性值的实例，不能为空。</param>
    /// <exception cref="ArgumentNullException">
    /// 当 <paramref name="target"/> 或 <paramref name="source"/> 为 <c>null</c> 时抛出。
    /// </exception>
    public static void UpdatePropertiesHighQualityFrom<T>(this T target, T source)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);

        if (ReferenceEquals(target, source))
        {
            return;
        }

        PropertyCopier<T>.CopyAction(target, source);
    }

    /// <summary>
    /// 高性能复制：利用表达式树缓存，将 <paramref name="source"/> 的属性快速应用到 <paramref name="target"/>，
    /// 同时排除指定的属性。
    /// </summary>
    /// <typeparam name="T">引用类型的对象。</typeparam>
    /// <param name="target">复制目标实例，不能为空。</param>
    /// <param name="source">提供属性值的实例，不能为空。</param>
    /// <param name="excludeProperties">要排除的属性名称集合。</param>
    /// <exception cref="ArgumentNullException">
    /// 当 <paramref name="target"/> 或 <paramref name="source"/> 为 <c>null</c> 时抛出。
    /// </exception>
    /// <remarks>
    /// 性能说明：此方法使用 HashSet 缓存排除属性组合，首次调用特定排除组合时会编译表达式树（约 0.1-1ms），
    /// 后续相同排除组合的调用性能与无过滤版本相当。频繁使用不同排除组合会增加内存占用。
    /// </remarks>
    public static void UpdatePropertiesHighQualityFrom<T>(this T target, T source, params string[] excludeProperties)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);

        if (ReferenceEquals(target, source))
        {
            return;
        }

        if (excludeProperties == null || excludeProperties.Length == 0)
        {
            PropertyCopier<T>.CopyAction(target, source);
            return;
        }

        var excludeSet = new HashSet<string>(excludeProperties, StringComparer.Ordinal);
        PropertyCopierWithExclusion<T>.GetCopyAction(excludeSet)(target, source);
    }

    /// <summary>
    /// 高性能复制：利用表达式树缓存，将 <paramref name="source"/> 的属性快速应用到 <paramref name="target"/>，
    /// 同时排除通过 Lambda 表达式指定的属性（类型安全）。
    /// </summary>
    /// <typeparam name="T">引用类型的对象。</typeparam>
    /// <param name="target">复制目标实例，不能为空。</param>
    /// <param name="source">提供属性值的实例，不能为空。</param>
    /// <param name="excludeSelectors">要排除的属性选择器，例如 x => x.Id, x => x.Name。</param>
    /// <exception cref="ArgumentNullException">
    /// 当 <paramref name="target"/> 或 <paramref name="source"/> 为 <c>null</c> 时抛出。
    /// </exception>
    public static void UpdatePropertiesHighQualityFrom<T>(this T target, T source, params Expression<Func<T, object?>>[] excludeSelectors)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);

        if (ReferenceEquals(target, source))
        {
            return;
        }

        if (excludeSelectors == null || excludeSelectors.Length == 0)
        {
            PropertyCopier<T>.CopyAction(target, source);
            return;
        }

        var excludeNames = excludeSelectors
            .Select(GetPropertyName)
            .Where(n => n != null)
            .ToArray();

        if (excludeNames.Length == 0)
        {
            PropertyCopier<T>.CopyAction(target, source);
            return;
        }

        var excludeSet = new HashSet<string>(excludeNames!, StringComparer.Ordinal);
        PropertyCopierWithExclusion<T>.GetCopyAction(excludeSet)(target, source);
    }

    private static string? GetPropertyName<T>(Expression<Func<T, object?>> selector)
    {
        var body = selector.Body;

        // 处理值类型装箱的 Convert 表达式
        if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
        {
            body = unary.Operand;
        }

        return body is MemberExpression member ? member.Member.Name : null;
    }

    /// <summary>
    /// 高性能复制：排除 <see cref="ObservableCollection{T}"/> 与 <see cref="BindingList{T}"/> 等集合类型，仅同步可写属性。集合类型同步内部元素，避免丢失现有绑定关系
    /// </summary>
    /// <typeparam name="T">引用类型的对象。</typeparam>
    /// <param name="target">复制目标实例，不能为空。</param>
    /// <param name="source">提供属性值的实例，不能为空。</param>
    /// <exception cref="ArgumentNullException">
    /// 当 <paramref name="target"/> 或 <paramref name="source"/> 为 <c>null</c> 时抛出。
    /// </exception>
    public static void UpdatePropertiesHighQualityExcludeGenericTypeFrom<T>(this T target, T source)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);

        if (ReferenceEquals(target, source))
        {
            return;
        }

        PropertyCopierExcludeGenericType<T>.CopyAction(target, source);
    }

    private static class PropertyCache<T>
    {
        public static readonly PropertyInfo[] WritableProperties = [.. typeof(T)
            .GetProperties()
            .Where(p => p.CanRead && p.CanWrite)];
    }

    private static class PropertyCopierWithExclusion<T>
    {
        private static readonly Dictionary<string, Action<T, T>> CachedActions = new();
        private static readonly object LockObj = new();

        public static Action<T, T> GetCopyAction(HashSet<string> excludeSet)
        {
            // 生成缓存 key（排序后拼接）
            var key = string.Join("|", excludeSet.OrderBy(x => x, StringComparer.Ordinal));

            lock (LockObj)
            {
                if (CachedActions.TryGetValue(key, out var cached))
                {
                    return cached;
                }

                var action = BuildCopyAction(excludeSet);
                CachedActions[key] = action;
                return action;
            }
        }

        private static Action<T, T> BuildCopyAction(HashSet<string> excludeSet)
        {
            var target = Expression.Parameter(typeof(T), "target");
            var source = Expression.Parameter(typeof(T), "source");

            var assigns = typeof(T)
                .GetProperties()
                .Where(p => p.CanRead && p.CanWrite && !excludeSet.Contains(p.Name))
                .Select(p =>
                    Expression.Assign(
                        Expression.Property(target, p),
                        Expression.Property(source, p)
                    )
                )
                .ToList();

            if (assigns.Count == 0)
            {
                return (_, _) => { };
            }

            var body = Expression.Block(assigns);
            return Expression.Lambda<Action<T, T>>(body, target, source).Compile();
        }
    }

    private static class PropertyCopier<T>
    {
        public static readonly Action<T, T> CopyAction;

        static PropertyCopier()
        {
            var target = Expression.Parameter(typeof(T), "target");
            var source = Expression.Parameter(typeof(T), "source");

            var assigns = typeof(T)
                .GetProperties()
                .Where(p => p.CanRead && p.CanWrite)
                .Select(p =>
                    Expression.Assign(
                        Expression.Property(target, p),
                        Expression.Property(source, p)
                    )
                );

            var body = Expression.Block(assigns);
            CopyAction = Expression.Lambda<Action<T, T>>(body, target, source).Compile();
        }
    }

    private static class PropertyCopierExcludeGenericType<T>
    {
        public static readonly Action<T, T> CopyAction;

        static PropertyCopierExcludeGenericType()
        {
            var target = Expression.Parameter(typeof(T), "target");
            var source = Expression.Parameter(typeof(T), "source");

            var statements = new List<Expression>();

            foreach (var p in typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite))
            {
                var targetProp = Expression.Property(target, p);
                var sourceProp = Expression.Property(source, p);

                // 针对 ObservableCollection<T> 和 BindingList<T> 特殊处理
                if (p.PropertyType.IsGenericType)
                {
                    var genType = p.PropertyType.GetGenericTypeDefinition();
                    if (
                        genType == typeof(ObservableCollection<>)
                        || genType == typeof(BindingList<>)
                    )
                    {
                        // if (target.Prop != null && source.Prop != null) { target.Prop.Clear();
                        // foreach (var item in source.Prop) target.Prop.Add(item); }

                        var itemType = p.PropertyType.GetGenericArguments()[0];
                        var enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);

                        var clearMethod = p.PropertyType.GetMethod("Clear")!;
                        var addMethod = p.PropertyType.GetMethod("Add")!;
                        var getEnumerator = enumerableType.GetMethod("GetEnumerator")!;

                        var enumeratorVar = Expression.Variable(
                            typeof(IEnumerator<>).MakeGenericType(itemType),
                            "enumerator"
                        );
                        var loopVar = Expression.Variable(itemType, "item");

                        var breakLabel = Expression.Label("LoopBreak");

                        var assignEnumerator = Expression.Assign(
                            enumeratorVar,
                            Expression.Call(sourceProp, getEnumerator)
                        );

                        var loop = Expression.Loop(
                            Expression.Block(
                                Expression.IfThenElse(
                                    Expression.Call(
                                        enumeratorVar,
                                        typeof(System.Collections.IEnumerator).GetMethod(
                                            "MoveNext"
                                        )!
                                    ),
                                    Expression.Block(
                                        [loopVar],
                                        Expression.Assign(
                                            loopVar,
                                            Expression.Property(enumeratorVar, "Current")
                                        ),
                                        Expression.Call(targetProp, addMethod, loopVar)
                                    ),
                                    Expression.Break(breakLabel)
                                )
                            ),
                            breakLabel
                        );

                        var block = Expression.Block(
                            [enumeratorVar],
                            Expression.IfThen(
                                Expression.AndAlso(
                                    Expression.NotEqual(targetProp, Expression.Constant(null)),
                                    Expression.NotEqual(sourceProp, Expression.Constant(null))
                                ),
                                Expression.Block(
                                    Expression.Call(targetProp, clearMethod),
                                    assignEnumerator,
                                    loop
                                )
                            )
                        );

                        statements.Add(block);
                        continue;
                    }
                }

                // 默认赋值
                statements.Add(Expression.Assign(targetProp, sourceProp));
            }

            var body = Expression.Block(statements);
            CopyAction = Expression.Lambda<Action<T, T>>(body, target, source).Compile();
        }
    }
}