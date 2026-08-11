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
        var missedFieldNames = GetMissedFieldNames().ToArray();
        if (missedFieldNames.Length > 0)
        {
            throw new DomainValidateStorageException(
                $"Не заполнены обязательные поля хранилища: {string.Join(", ", missedFieldNames)}");
        }

        return Storage.Create(Name!, Description!, userName);
    }

    private IEnumerable<string> GetMissedFieldNames()
    {
        if (Name.IsNullOrEmpty())
        {
            yield return nameof(Name);
        }

        if (Description.IsNullOrEmpty())
        {
            yield return nameof(Description);
        }
    }
}
