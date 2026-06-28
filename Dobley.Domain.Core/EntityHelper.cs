using System.ComponentModel.DataAnnotations;

namespace Dobley.Domain.Core;

public static class EntityHelper
{
    public static bool IsNullOrEmpty(this string? value) => value == null || string.IsNullOrEmpty(value);

    public static bool IsNullOrEmpty<TEntity>(this IEnumerable<TEntity>? value) => value == null || !value.Any();

    public static string GetDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field != null)
        {
            if (Attribute.GetCustomAttribute(field, typeof(DisplayAttribute)) is DisplayAttribute attr)
            {
                return attr.Name!;
            }
        }

        return value.ToString();
    }
}