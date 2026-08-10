namespace Dobley.Domain.Core.Repositories.Storages;

public class StorageFilter(params int[] ids)
{
    public IReadOnlyList<int>? Ids { get; set; } = ids;

    public IReadOnlyList<string>? UserNames { get; set; }

    public StorageFilter SetIds(IReadOnlyList<int> ids)
    {
        Ids = ids;

        return this;
    }

    public StorageFilter SetUserNames(IReadOnlyList<string> userNames)
    {
        UserNames = userNames;

        return this;
    }
}
