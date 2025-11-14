using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Extensions;

public static class JsonExtensions
{
    /// <summary>
    /// 预配置的序列化选项，统一输出风格并支持全量 Unicode。
    /// </summary>
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), // 支持中文输出
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    /// <summary>
    /// 将对象序列化为 JSON 字符串，自动使用默认配置并对 <c>null</c> 做出友好处理。
    /// </summary>
    /// <typeparam name="T">待序列化的类型。</typeparam>
    /// <param name="value">扩展调用者，允许为 <c>null</c>。</param>
    /// <returns>序列化后的 JSON 字符串（当 <paramref name="value"/> 为 <c>null</c> 时返回 <c>"null"</c>）。</returns>
    public static string ToJson<T>(this T value)
    {
        return JsonSerializer.Serialize(value, DefaultOptions);
    }

    /// <summary>
    /// 将对象序列化为 JSON 字符串，支持传入自定义选项并在 <paramref name="options"/> 为空时回退到默认配置。
    /// </summary>
    /// <typeparam name="T">待序列化的类型。</typeparam>
    /// <param name="value">扩展调用者，允许为 <c>null</c>。</param>
    /// <param name="options">自定义序列化选项，如果为 <c>null</c> 则使用默认配置。</param>
    /// <returns>序列化后的 JSON 字符串。</returns>
    public static string ToJson<T>(this T value, JsonSerializerOptions? options)
    {
        return JsonSerializer.Serialize(value, options ?? DefaultOptions);
    }

    /// <summary>
    /// 依赖 JSON 序列化实现对象的深拷贝。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T DeepClone<T>(this T obj)
    => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(obj))!;
}