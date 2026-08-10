using Dobley.Domain.Core.Entities.Products;

namespace Dobley.Domain.Core.Tests.Products;

public record CreatingProductTestCase(string TestName, string? Name, string? Description, string? Category,
    decimal Unit, string? UnitType, decimal Price, string? Barcode, int StorageId, bool IsValid,
    Category? ExpectedCategory = null, UnitType? ExpectedUnitType = null, int? ExpectedStorageId = null)
{
    public override string ToString() => TestName;
}

public class CreatingProductTestDataGenerator
    : DataGenerator<CreatingProductTestCase>
{
    protected override IEnumerable<CreatingProductTestCase> GetData()
    {
        yield return new CreatingProductTestCase(
            TestName: "1.1 Корректный продукт с дефолтными значениями",
            Name: "Milk",
            Description: "Fresh milk",
            Category: Category.Dairy.ToString(),
            Unit: 1,
            UnitType: UnitType.Liters.ToString(),
            Price: 120,
            Barcode: "4600000000000",
            StorageId: 1,
            IsValid: true,
            ExpectedCategory: Category.Dairy,
            ExpectedUnitType: UnitType.Liters,
            ExpectedStorageId: 1);

        yield return new CreatingProductTestCase(
            TestName: "1.2 Корректный продукт с другой категорией и единицей измерения",
            Name: "Water",
            Description: "Still water",
            Category: Category.Beverages.ToString(),
            Unit: 6,
            UnitType: UnitType.Pieces.ToString(),
            Price: 300,
            Barcode: "4600000000001",
            StorageId: 2,
            IsValid: true,
            ExpectedCategory: Category.Beverages,
            ExpectedUnitType: UnitType.Pieces,
            ExpectedStorageId: 2);

        yield return new CreatingProductTestCase(
            TestName: "1.3 Некорректное название продукта",
            Name: null,
            Description: "Fresh milk",
            Category: Category.Dairy.ToString(),
            Unit: 1,
            UnitType: UnitType.Liters.ToString(),
            Price: 120,
            Barcode: "4600000000000",
            StorageId: 1,
            IsValid: false);

        yield return new CreatingProductTestCase(
            TestName: "1.4 Некорректное описание продукта",
            Name: "Milk",
            Description: null,
            Category: Category.Dairy.ToString(),
            Unit: 1,
            UnitType: UnitType.Liters.ToString(),
            Price: 120,
            Barcode: "4600000000000",
            StorageId: 1,
            IsValid: false);

        yield return new CreatingProductTestCase(
            TestName: "1.5 Некорректная категория продукта",
            Name: "Milk",
            Description: "Fresh milk",
            Category: "Unknown",
            Unit: 1,
            UnitType: UnitType.Liters.ToString(),
            Price: 120,
            Barcode: "4600000000000",
            StorageId: 1,
            IsValid: false);

        yield return new CreatingProductTestCase(
            TestName: "1.6 Некорректное количество продукта",
            Name: "Milk",
            Description: "Fresh milk",
            Category: Category.Dairy.ToString(),
            Unit: -1,
            UnitType: UnitType.Liters.ToString(),
            Price: 120,
            Barcode: "4600000000000",
            StorageId: 1,
            IsValid: false);

        yield return new CreatingProductTestCase(
            TestName: "1.7 Некорректная единица измерения продукта",
            Name: "Milk",
            Description: "Fresh milk",
            Category: Category.Dairy.ToString(),
            Unit: 1,
            UnitType: "Unknown",
            Price: 120,
            Barcode: "4600000000000",
            StorageId: 1,
            IsValid: false);

        yield return new CreatingProductTestCase(
            TestName: "1.8 Некорректная цена продукта",
            Name: "Milk",
            Description: "Fresh milk",
            Category: Category.Dairy.ToString(),
            Unit: 1,
            UnitType: UnitType.Liters.ToString(),
            Price: -1,
            Barcode: "4600000000000",
            StorageId: 1,
            IsValid: false);

        yield return new CreatingProductTestCase(
            TestName: "1.9 Некорректный штрихкод продукта",
            Name: "Milk",
            Description: "Fresh milk",
            Category: Category.Dairy.ToString(),
            Unit: 1,
            UnitType: UnitType.Liters.ToString(),
            Price: 120,
            Barcode: null,
            StorageId: 1,
            IsValid: false);

        yield return new CreatingProductTestCase(
            TestName: "1.10 Некорректный идентификатор хранилища продукта",
            Name: "Milk",
            Description: "Fresh milk",
            Category: Category.Dairy.ToString(),
            Unit: 1,
            UnitType: UnitType.Liters.ToString(),
            Price: 120,
            Barcode: "4600000000000",
            StorageId: 0,
            IsValid: false);
    }
}
