using Dobley.Domain.Core.Errors.Entities;
using Storage = Dobley.Domain.Core.Entities.Storages.Storage;

namespace Dobley.Domain.Core.Tests.Storages;

public class CreatingStorageTests
{
    [Theory]
    [ClassData(typeof(CreatingStorageTestDataGenerator))]
    public void Create_ReturnsStorageOrThrowsDomainException(CreatingStorageTestCase testCase)
    {
        if (!testCase.IsValid)
        {
            Assert.Throws<DomainValidateStorageException>(() => CreateStorage(testCase));
            return;
        }

        var storage = CreateStorage(testCase);

        Assert.Equal(testCase.ExpectedName, storage.Name);
        Assert.Equal(testCase.ExpectedDescription, storage.Description);
        Assert.Equal(testCase.ExpectedUserName, storage.UserName);
    }

    private static Storage CreateStorage(CreatingStorageTestCase testCase)
        => Storage.Create(testCase.Name!, testCase.Description!, testCase.UserName!);
}
