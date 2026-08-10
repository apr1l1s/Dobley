namespace Dobley.Domain.Core.Tests.Storages;

public record CreatingStorageTestCase(string TestName, string? Name, string? Description, string? UserName, bool IsValid,
    string? ExpectedName = null, string? ExpectedDescription = null, string? ExpectedUserName = null)
{
    public override string ToString() => TestName;
}

public class CreatingStorageTestDataGenerator
    : DataGenerator<CreatingStorageTestCase>
{
    protected override IEnumerable<CreatingStorageTestCase> GetData()
    {
        yield return new CreatingStorageTestCase(
            TestName: "1.1 Корректное хранилище с дефолтными значениями",
            Name: "Fridge",
            Description: "Kitchen fridge",
            UserName: "demo",
            IsValid: true,
            ExpectedName: "Fridge",
            ExpectedDescription: "Kitchen fridge",
            ExpectedUserName: "demo");

        yield return new CreatingStorageTestCase(
            TestName: "1.2 Корректное хранилище с пользовательскими значениями",
            Name: "Freezer",
            Description: "Garage freezer",
            UserName: "owner",
            IsValid: true,
            ExpectedName: "Freezer",
            ExpectedDescription: "Garage freezer",
            ExpectedUserName: "owner");

        yield return new CreatingStorageTestCase(
            TestName: "1.3 Некорректное название хранилища",
            Name: null,
            Description: "Kitchen fridge",
            UserName: "demo",
            IsValid: false);

        yield return new CreatingStorageTestCase(
            TestName: "1.4 Некорректное описание хранилища",
            Name: "Fridge",
            Description: null,
            UserName: "demo",
            IsValid: false);

        yield return new CreatingStorageTestCase(
            TestName: "1.5 Некорректный владелец хранилища",
            Name: "Fridge",
            Description: "Kitchen fridge",
            UserName: null,
            IsValid: false);
    }
}
