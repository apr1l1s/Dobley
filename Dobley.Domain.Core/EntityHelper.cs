using Dobley.Domain.Core.Repositories;

namespace Dobley.Domain.Core;

public static class EntityHelper
{
    public static bool IsNullOrEmpty(this string? value) => value == null || string.IsNullOrEmpty(value);

    public static bool IsNullOrEmpty<TEntity>(this IEnumerable<TEntity>? value) => value == null || !value.Any();
}