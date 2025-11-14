namespace Extensions;

public static class ICollectionExtensions
{
    /// <summary>
    /// 批量添加
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="coll"></param>
    /// <param name="items"></param>
    public static void AddRange<T>(this ICollection<T> coll, IEnumerable<T> items)
    {
        foreach (var item in items) coll.Add(item);
    }
}