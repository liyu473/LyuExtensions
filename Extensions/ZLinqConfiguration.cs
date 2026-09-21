namespace LyuExtensions.Extensions;

/// <summary>
/// 提供 ZLinq 的编译期配置说明。
/// </summary>
public static class ZLinqConfiguration
{
    /// <summary>
    /// 说明当前程序集提供了 ZLinq Drop-in 配置。
    /// Drop-in 由源生成器在编译期完成，运行时不能重新绑定已经编译的 LINQ 调用。
    /// </summary>
    public static bool IsAvailable => true;
}
