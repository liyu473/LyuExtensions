using System.ComponentModel;
using System.Reflection;

namespace Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// 获取单个枚举值的描述（泛型枚举）
    /// </summary>
    public static string GetEnumDescription<TEnum>(this TEnum value)
        where TEnum : Enum
    {
        FieldInfo? field = typeof(TEnum).GetField(value.ToString());
        if (field != null)
        {
            DescriptionAttribute? attr = field.GetCustomAttribute<DescriptionAttribute>();
            if (attr != null)
                return attr.Description;
        }

        return value.ToString();
    }

    /// <summary>
    /// 获取单个枚举值的描述(具体泛型)
    /// </summary>
    public static string GetEnumDescription(this Enum value)
    {
        FieldInfo? field = value.GetType().GetField(value.ToString());
        if (field != null)
        {
            var attr = field.GetCustomAttribute<DescriptionAttribute>();
            if (attr != null)
                return attr.Description;
        }
        return value.ToString();
    }
}