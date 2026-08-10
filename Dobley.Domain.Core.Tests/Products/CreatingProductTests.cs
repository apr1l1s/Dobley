using Dobley.Domain.Core.Errors.Entities;
using Product = Dobley.Domain.Core.Entities.Products.Product;

namespace Dobley.Domain.Core.Tests.Products;

public class CreatingProductTests
{
    [Theory]
    [ClassData(typeof(CreatingProductTestDataGenerator))]
    public void Create_ReturnsProductOrThrowsDomainException(CreatingProductTestCase testCase)
    {
        if (!testCase.IsValid)
        {
            Assert.Throws<DomainValidateProductException>(() => CreateProduct(testCase));
            return;
        }

        var product = CreateProduct(testCase);

        Assert.Equal(testCase.ExpectedCategory, product.Category);
        Assert.Equal(testCase.ExpectedUnitType, product.UnitType);
        Assert.Equal(testCase.ExpectedStorageId, product.StorageId);
    }

    private static Product CreateProduct(CreatingProductTestCase testCase)
        => Product.Create(testCase.Name!, testCase.Description!, testCase.Category!, testCase.Unit,
            testCase.UnitType!, testCase.Price, testCase.Barcode!, testCase.StorageId);
}
