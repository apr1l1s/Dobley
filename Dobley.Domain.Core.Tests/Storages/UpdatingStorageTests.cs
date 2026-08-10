using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Tests.Storages;

public class UpdatingStorageTests
{
    [Theory]
    [ClassData(typeof(UpdatingStorageTestDataGenerator))]
    public void Update_ReturnsStorageOrThrowsDomainException(UpdatingStorageTestCase testCase)
    {
        if (!testCase.IsValid)
        {
            Assert.Throws<DomainValidateStorageException>(() =>
                testCase.Storage.Update(testCase.Name, testCase.Description, null));
            return;
        }

        testCase.Storage.Update(testCase.Name, testCase.Description, null);

        Assert.Equal(testCase.ExpectedName, testCase.Storage.Name);
        Assert.Equal(testCase.ExpectedDescription, testCase.Storage.Description);
    }
}
