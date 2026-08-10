using Dobley.Domain.Core.Entities.Storages;

namespace Dobley.Domain.Core.Tests.Builders;

public static class StorageBuilder
{
    public static int LastId { get; private set; }

    public static Storage Build(int? id = null, string name = "Fridge", string description = "Kitchen fridge",
        string userName = "demo")
    {
        var storage = Storage.Create(name, description, userName);
        storage.Id = id ?? ++LastId;

        return storage;
    }
}
