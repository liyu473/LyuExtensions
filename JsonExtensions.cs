using System.Diagnostics.CodeAnalysis;
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
    /// 从 JSON 字符串反序列化为指定类型。
    /// </summary>
    public static T? FromJson<T>(this string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;
        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }

    /// <summary>
    /// 从 JSON 字符串反序列化为指定类型，支持传入自定义选项并在 <paramref name="options"/> 为空时回退到默认配置。
    /// </summary>
    /// <typeparam name="T">目标类型。</typeparam>
    /// <param name="json">JSON 字符串。</param>
    /// <param name="options">自定义反序列化选项，如果为 <c>null</c> 则使用默认配置。</param>
    /// <returns>反序列化后的对象，如果输入为空或反序列化失败则返回 <c>default</c>。</returns>
    public static T? FromJson<T>(this string json, JsonSerializerOptions? options)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;
        return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
    }

    /// <summary>
    /// 安全地尝试从 JSON 反序列化，失败时不抛异常。
    /// </summary>
    public static bool TryFromJson<T>(this string json, [NotNullWhen(true)] out T? result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(json))
            return false;
        try
        {
            result = JsonSerializer.Deserialize<T>(json, DefaultOptions);
            return result != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 从 JSON 字符串中提取指定路径的片段。路径格式："user.name" 或 "items[0].price"
    /// </summary>
    public static string? GetJsonFragment(this string json, string path)
    {
        if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(path))
            return null;
        try
        {
            using var document = JsonDocument.Parse(json);
            var element = NavigateToElement(document.RootElement, path);
            return element?.GetRawText();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 从 JSON 字符串中提取指定路径的片段并反序列化为指定类型。
    /// </summary>
    public static T? GetJsonValue<T>(this string json, string path)
    {
        var fragment = json.GetJsonFragment(path);
        if (fragment == null)
            return default;
        try
        {
            return JsonSerializer.Deserialize<T>(fragment, DefaultOptions);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// 检查 JSON 中是否存在指定路径。
    /// </summary>
    public static bool HasJsonPath(this string json, string path)
    {
        if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(path))
            return false;
        try
        {
            using var document = JsonDocument.Parse(json);
            var element = NavigateToElement(document.RootElement, path);
            return element.HasValue;
        }
        catch
        {
            return false;
        }
    }

    private static JsonElement? NavigateToElement(JsonElement element, string path)
    {
        var parts = path.Split('.');
        var current = element;

        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
                continue;

            if (part.Contains('[') && part.Contains(']'))
            {
                var propertyName = part[..part.IndexOf('[')];
                var indexStr = part[(part.IndexOf('[') + 1)..part.IndexOf(']')];

                if (!string.IsNullOrEmpty(propertyName))
                {
                    if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(propertyName, out current))
                        return null;
                }

                if (int.TryParse(indexStr, out var index))
                {
                    if (current.ValueKind != JsonValueKind.Array)
                        return null;
                    var arrayLength = current.GetArrayLength();
                    if (index < 0 || index >= arrayLength)
                        return null;
                    current = current[index];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(part, out current))
                    return null;
            }
        }

        return current;
    }
}