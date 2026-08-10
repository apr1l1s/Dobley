using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Tests.Builders;

namespace Dobley.Domain.Core.Tests.Products;

public record UpdatingProductTestCase(string TestName, Product Product, decimal? Price, bool IsValid)
{
    public override string ToString() => TestName;
}

public class UpdatingProductTestDataGenerator
    : DataGenerator<UpdatingProductTestCase>
{
    protected override IEnumerable<UpdatingProductTestCase> GetData()
    {
        yield return new UpdatingProductTestCase(
            TestName: "2.1 Корректное обновление цены продукта",
            Product: ProductBuilder.Build(),
            Price: 250,
            IsValid: true);

        yield return new UpdatingProductTestCase(
            TestName: "2.2 Корректное обновление цены продукта в ноль",
            Product: ProductBuilder.Build(),
            Price: 0,
            IsValid: true);

        yield return new UpdatingProductTestCase(
            TestName: "2.3 Некорректное обновление цены продукта",
            Product: ProductBuilder.Build(),
            Price: -1,
            IsValid: false);
    }
}
