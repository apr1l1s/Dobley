using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Tests.Products;

public class UpdatingProductTests
{
    [Theory]
    [ClassData(typeof(UpdatingProductTestDataGenerator))]
    public void Update_ReturnsProductOrThrowsDomainException(UpdatingProductTestCase testCase)
    {
        var product = testCase.Product;

        if (!testCase.IsValid)
        {
            Assert.Throws<DomainValidateProductException>(() =>
                product.Update(null, null, null, null, null, testCase.Price, null));
            return;
        }

        product.Update(null, null, null, null, null, testCase.Price, null);

        Assert.Equal(testCase.Price, product.Price);
    }
}
