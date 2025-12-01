using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Extensions;

public static class StringExtensions
{
    /// <summary>
    /// 判断字符串是否为 null 或空白。
    /// </summary>
    /// <param name="value">要检查的字符串。</param>
    /// <returns>如果字符串为 null 或仅包含空白字符，则返回 true；否则返回 false。</returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// 判断字符串是否为 null 或空。
    /// </summary>
    /// <param name="value">要检查的字符串。</param>
    /// <returns>如果字符串为 null 或空，则返回 true；否则返回 false。</returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// 高性能字符串拼接。
    /// 如果仅拼接一次，使用 String.Concat；
    /// 如果用于累计拼接，可以配合 StringBuilder。
    /// </summary>
    /// <param name="source">原始字符串。</param>
    /// <param name="append">要拼接的内容。</param>
    /// <returns>拼接后的新字符串。</returns>
    public static string Append(this string? source, string? append)
    {
        if (string.IsNullOrEmpty(append))
            return source ?? string.Empty;

        if (string.IsNullOrEmpty(source))
            return append;

        return string.Concat(source, append);
    }



    /// <summary>
    /// 使用 StringBuilder 进行可重复拼接（适合需要多次追加的场景）。
    /// </summary>
    /// <param name="builder">StringBuilder 实例。</param>
    /// <param name="value">要追加的字符串。</param>
    /// <returns>同一个 StringBuilder，用于链式调用。</returns>
    public static StringBuilder AppendSmart(this StringBuilder builder, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            builder.Append(value);

        return builder;
    }
}