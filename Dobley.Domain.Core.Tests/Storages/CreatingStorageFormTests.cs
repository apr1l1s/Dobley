using Dobley.Domain.Core.Errors.Entities;
using Dobley.Domain.Core.Forms;

namespace Dobley.Domain.Core.Tests.Storages;

public class CreatingStorageFormTests
{
    [Fact]
    public void ToEntity_ThrowsDomainExceptionWithMissedFieldNames()
    {
        var form = new StorageForm
        {
            Name = null,
            Description = null
        };

        var exception = Assert.Throws<DomainValidateStorageException>(() => form.ToEntity("demo"));

        Assert.Equal("Не заполнены обязательные поля хранилища: Name, Description", exception.Message);
    }
}
