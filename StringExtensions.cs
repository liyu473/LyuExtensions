namespace Extensions;

public static class StringExtensions
{
    /// <summary>
    /// 判断字符串是否为 null 或空白。
    /// </summary>
    /// <param name="value">要检查的字符串。</param>
    /// <returns>如果字符串为 null 或仅包含空白字符，则返回 true；否则返回 false。</returns>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }
}