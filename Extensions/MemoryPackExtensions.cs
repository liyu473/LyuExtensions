using System.Diagnostics.CodeAnalysis;
using MemoryPack;

namespace LyuExtensions.Extensions;

public static class MemoryPackExtensions
{
    /// <summary>
    /// 将对象序列化为 MemoryPack 字节数组。
    /// </summary>
    /// <typeparam name="T">待序列化的类型，必须标记 [MemoryPackable] 特性。</typeparam>
    /// <param name="value">扩展调用者，允许为 <c>null</c>。</param>
    /// <returns>序列化后的字节数组（当 <paramref name="value"/> 为 <c>null</c> 时返回空数组）。</returns>
    public static byte[] ToMemoryPack<T>(this T? value)
    {
        if (value == null)
            return [];
        return MemoryPackSerializer.Serialize(value);
    }

    /// <summary>
    /// 从 MemoryPack 字节数组反序列化为指定类型。
    /// </summary>
    /// <typeparam name="T">目标类型，必须标记 [MemoryPackable] 特性。</typeparam>
    /// <param name="bytes">字节数组。</param>
    /// <returns>反序列化后的对象，如果输入为空或反序列化失败则返回 <c>default</c>。</returns>
    public static T? FromMemoryPack<T>(this byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return default;
        return MemoryPackSerializer.Deserialize<T>(bytes);
    }

    /// <summary>
    /// 从 MemoryPack 字节数组反序列化为指定类型（ReadOnlySpan 版本，零拷贝）。
    /// </summary>
    /// <typeparam name="T">目标类型，必须标记 [MemoryPackable] 特性。</typeparam>
    /// <param name="bytes">字节数组的只读跨度。</param>
    /// <returns>反序列化后的对象，如果输入为空或反序列化失败则返回 <c>default</c>。</returns>
    public static T? FromMemoryPack<T>(this ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
            return default;
        return MemoryPackSerializer.Deserialize<T>(bytes);
    }

    /// <summary>
    /// 安全地尝试从 MemoryPack 字节数组反序列化，失败时不抛异常。
    /// </summary>
    /// <typeparam name="T">目标类型，必须标记 [MemoryPackable] 特性。</typeparam>
    /// <param name="bytes">字节数组。</param>
    /// <param name="result">反序列化结果。</param>
    /// <returns>如果反序列化成功返回 <c>true</c>，否则返回 <c>false</c>。</returns>
    public static bool TryFromMemoryPack<T>(this byte[] bytes, [NotNullWhen(true)] out T? result)
    {
        result = default;
        if (bytes == null || bytes.Length == 0)
            return false;
        try
        {
            result = MemoryPackSerializer.Deserialize<T>(bytes);
            return result != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 安全地尝试从 MemoryPack 字节数组反序列化，失败时不抛异常（ReadOnlySpan 版本）。
    /// </summary>
    /// <typeparam name="T">目标类型，必须标记 [MemoryPackable] 特性。</typeparam>
    /// <param name="bytes">字节数组的只读跨度。</param>
    /// <param name="result">反序列化结果。</param>
    /// <returns>如果反序列化成功返回 <c>true</c>，否则返回 <c>false</c>。</returns>
    public static bool TryFromMemoryPack<T>(this ReadOnlySpan<byte> bytes, [NotNullWhen(true)] out T? result)
    {
        result = default;
        if (bytes.IsEmpty)
            return false;
        try
        {
            result = MemoryPackSerializer.Deserialize<T>(bytes);
            return result != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 将对象序列化为 MemoryPack 字节数组并转换为 Base64 字符串（便于存储和传输）。
    /// </summary>
    /// <typeparam name="T">待序列化的类型，必须标记 [MemoryPackable] 特性。</typeparam>
    /// <param name="value">扩展调用者，允许为 <c>null</c>。</param>
    /// <returns>Base64 编码的字符串（当 <paramref name="value"/> 为 <c>null</c> 时返回空字符串）。</returns>
    public static string ToMemoryPackBase64<T>(this T? value)
    {
        if (value == null)
            return string.Empty;
        var bytes = MemoryPackSerializer.Serialize(value);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// 从 Base64 字符串反序列化为指定类型。
    /// </summary>
    /// <typeparam name="T">目标类型，必须标记 [MemoryPackable] 特性。</typeparam>
    /// <param name="base64">Base64 编码的字符串。</param>
    /// <returns>反序列化后的对象，如果输入为空或反序列化失败则返回 <c>default</c>。</returns>
    public static T? FromMemoryPackBase64<T>(this string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
            return default;
        try
        {
            var bytes = Convert.FromBase64String(base64);
            return MemoryPackSerializer.Deserialize<T>(bytes);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// 安全地尝试从 Base64 字符串反序列化，失败时不抛异常。
    /// </summary>
    /// <typeparam name="T">目标类型，必须标记 [MemoryPackable] 特性。</typeparam>
    /// <param name="base64">Base64 编码的字符串。</param>
    /// <param name="result">反序列化结果。</param>
    /// <returns>如果反序列化成功返回 <c>true</c>，否则返回 <c>false</c>。</returns>
    public static bool TryFromMemoryPackBase64<T>(this string base64, [NotNullWhen(true)] out T? result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(base64))
            return false;
        try
        {
            var bytes = Convert.FromBase64String(base64);
            result = MemoryPackSerializer.Deserialize<T>(bytes);
            return result != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 深拷贝对象（通过序列化和反序列化实现）。
    /// </summary>
    /// <typeparam name="T">对象类型，必须标记 [MemoryPackable] 特性。</typeparam>
    /// <param name="value">待拷贝的对象。</param>
    /// <returns>深拷贝后的新对象，如果输入为 <c>null</c> 则返回 <c>default</c>。</returns>
    public static T? DeepClone<T>(this T? value)
    {
        if (value == null)
            return default;
        var bytes = MemoryPackSerializer.Serialize(value);
        return MemoryPackSerializer.Deserialize<T>(bytes);
    }
}
