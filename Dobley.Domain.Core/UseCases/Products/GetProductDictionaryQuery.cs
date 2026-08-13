using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Dobley.Domain.Core.Entities.Products;

namespace Dobley.Domain.Core.UseCases.Products;

public enum ProductDictionaryKind
{
    Categories = 1,
    UnitTypes = 2
}

public record ProductDictionaryItem(string Name, string DisplayName);

public record GetProductDictionaryQuery(ProductDictionaryKind Kind)
    : IUseCase<IReadOnlyList<ProductDictionaryItem>>;

public record GetProductDictionaryQueryHandler
    : IUseCaseHandler<GetProductDictionaryQuery, IReadOnlyList<ProductDictionaryItem>>
{
    public Task<IReadOnlyList<ProductDictionaryItem>> Handle(GetProductDictionaryQuery request,
        CancellationToken cancellationToken)
        => Task.FromResult(request.Kind switch
        {
            ProductDictionaryKind.Categories => GetDictionary<Category>(),
            ProductDictionaryKind.UnitTypes => GetDictionary<UnitType>(),
            _ => []
        });

    private static IReadOnlyList<ProductDictionaryItem> GetDictionary<TEnum>()
        where TEnum : struct, Enum
        => Enum.GetValues<TEnum>()
            .Select(value => new ProductDictionaryItem(value.ToString(), GetDisplayName(value)))
            .ToArray();

    private static string GetDisplayName<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();

        return member?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString();
    }
}
