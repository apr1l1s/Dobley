using Dobley.Domain.Core.Entities.Storages;

namespace Dobley.Endpoints.Api.Dto;

public record StorageResponse(int Id, string UserName, string Name, string Description)
{
    public static StorageResponse Create(Storage storage)
        => new(storage.Id, storage.UserName, storage.Name, storage.Description);
}
