using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Forms;

public class StorageForm
{
    public int? Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? UserName { get; set; }

    public static StorageForm ToForm(Storage storage)
        => new()
        {
            Id = storage.Id,
            Name = storage.Name,
            Description = storage.Description,
            UserName = storage.UserName
        };

    public Storage ToEntity(string userName)
    {
        if (Name.IsNullOrEmpty() || Description.IsNullOrEmpty())
        {
            throw new DomainValidateStorageException(
                "Не все обязательные поля хранилища заполнены");
        }

        return Storage.Create(Name!, Description!, userName);
    }
}
