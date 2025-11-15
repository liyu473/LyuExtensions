using MemoryPack;
using System.Text.Json;

namespace Extensions;

public static class CloneExtension
{
    /// <summary>
    /// 依赖 MemoryPack 二进制极致性能序列化工具实现对象的深拷贝。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T? ZClone<T>(this T obj)
    {
        var bin = MemoryPackSerializer.Serialize(obj);
        return MemoryPackSerializer.Deserialize<T>(bin);
    }

    /// <summary>
    /// 依赖 JSON 序列化实现对象的深拷贝。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T? JClone<T>(this T obj)
    => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(obj));
}