using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Tests.Builders;

namespace Dobley.Domain.Core.Tests.Storages;

public record UpdatingStorageTestCase(string TestName, Storage Storage, string? Name, string? Description, bool IsValid,
    string? ExpectedName = null, string? ExpectedDescription = null)
{
    public override string ToString() => TestName;
}

public class UpdatingStorageTestDataGenerator
    : DataGenerator<UpdatingStorageTestCase>
{
    protected override IEnumerable<UpdatingStorageTestCase> GetData()
    {
        yield return new UpdatingStorageTestCase(
            TestName: "2.1 Корректное обновление названия хранилища",
            Storage: StorageBuilder.Build(),
            Name: "Freezer",
            Description: null,
            IsValid: true,
            ExpectedName: "Freezer",
            ExpectedDescription: "Kitchen fridge");

        yield return new UpdatingStorageTestCase(
            TestName: "2.2 Корректное обновление описания хранилища",
            Storage: StorageBuilder.Build(),
            Name: null,
            Description: "Garage freezer",
            IsValid: true,
            ExpectedName: "Fridge",
            ExpectedDescription: "Garage freezer");

        yield return new UpdatingStorageTestCase(
            TestName: "2.3 Некорректное обновление названия хранилища",
            Storage: StorageBuilder.Build(),
            Name: string.Empty,
            Description: null,
            IsValid: false);

        yield return new UpdatingStorageTestCase(
            TestName: "2.4 Некорректное обновление описания хранилища",
            Storage: StorageBuilder.Build(),
            Name: null,
            Description: string.Empty,
            IsValid: false);
    }
}
