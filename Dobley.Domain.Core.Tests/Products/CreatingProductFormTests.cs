using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Errors.Entities;
using Dobley.Domain.Core.Forms;

namespace Dobley.Domain.Core.Tests.Products;

public class CreatingProductFormTests
{
    [Fact]
    public void ToEntity_ThrowsDomainExceptionWithMissedFieldNames()
    {
        var form = new ProductForm
        {
            Name = "Milk",
            Description = null,
            Category = Category.Dairy,
            Unit = null,
            UnitType = UnitType.Liters,
            Price = 120,
            Barcode = null,
            StorageId = 1
        };

        var exception = Assert.Throws<DomainValidateProductException>(form.ToEntity);

        Assert.Equal("Не заполнены обязательные поля продукта: Description, Unit, Barcode", exception.Message);
    }
}
