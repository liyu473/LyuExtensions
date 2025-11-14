namespace Extensions;

public static class DoubleExtension
{
    /// <summary>
    /// 将当前 double 数值四舍五入到指定的小数位数。
    /// </summary>
    /// <param name="d">需要四舍五入的 double 数值。</param>
    /// <param name="digits">要保留的小数位数。 默认值为 2 位小数。 取值范围：0-15。</param>
    /// <returns>返回四舍五入后的 double 值。 例如： 3.14159.Round(2) 返回 3.14 3.14159.Round(4) 返回 3.1416</returns>
    public static double Round(this double d, int digits = 2) => Math.Round(d, digits);

    /// <summary>
    /// 将当前 double 数值转换为百分比字符串表示形式，并保留指定的小数位数。 自动将数值乘以 100，并在末尾添加百分号。
    /// </summary>
    /// <param name="d">需要转换的 double 数值。 例如：0.1234 表示 12.34%</param>
    /// <param name="digits">百分比中保留的小数位数。 默认值为 2 位小数。 取值范围：0-15。</param>
    /// <returns>
    /// 返回百分比字符串，包含百分号。 例如： 0.1234.ToPercent() 返回 "12.34 %" 0.12345.ToPercent(3) 返回 "12.345 %"
    /// 1.ToPercent(0) 返回 "100 %"
    /// </returns>
    public static string ToPercent(this double d, int digits = 2) =>
        $"{(d * 100).Round(digits)} %";
}