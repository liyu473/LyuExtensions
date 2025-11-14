using System.Threading;

namespace Extensions;

public static class IEnumerableExtensions
{
    /// <summary>
    /// 依次遍历集合中的每个元素并执行指定动作。
    /// </summary>
    /// <typeparam name="T">集合元素类型。</typeparam>
    /// <param name="values">要遍历的集合。</param>
    /// <param name="action">对每个元素执行的动作。</param>
    /// <returns>原始集合（便于链式调用或继续使用）。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="values"/> 或 <paramref name="action"/> 为 <c>null</c> 时抛出。</exception>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> values, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in values)
        {
            action(item);
        }

        return values;
    }

    /// <summary>
    /// 依次遍历集合中的每个元素并执行指定的异步操作，采用顺序等待保证执行顺序。
    /// </summary>
    /// <typeparam name="T">集合元素类型。</typeparam>
    /// <param name="values">要遍历的集合。</param>
    /// <param name="func">对每个元素执行的异步操作。</param>
    /// <param name="cancellationToken">可选的取消标记，用于提前终止遍历。</param>
    /// <returns>原始集合（便于链式调用或继续使用）。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="values"/> 或 <paramref name="func"/> 为 <c>null</c> 时抛出。</exception>
    public static async Task<IEnumerable<T>> ForEachAsync<T>(this IEnumerable<T> values, Func<T, Task> func, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(func);

        foreach (var item in values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await func(item).ConfigureAwait(false);
        }

        return values;
    }
}